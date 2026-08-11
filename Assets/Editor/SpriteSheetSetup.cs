using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// Configura e fatia as sprite sheets do projeto.
///
/// Duas coisas que precisam ser feitas com cuidado aqui:
///
/// 1. O tamanho de frame é descoberto contando as "ilhas" de pixels opacos,
///    não chutado. As folhas do pacote têm larguras de frame diferentes por
///    animação (a Kaida vai de 64 a 96 px), então um valor fixo erraria.
///
/// 2. O pivô é calculado por folha, colocando os pés no mesmo ponto em todas
///    as animações. As folhas foram exportadas com recorte diferente: o idle
///    tem 16 px de folga embaixo e o pulo tem 0. Com o pivô padrão a
///    personagem afundaria quase uma unidade ao trocar de animação.
/// </summary>
public static class SpriteSheetSetup
{
    public const int PixelsPerUnit = 16;

    [MenuItem("Kaida/1. Configurar sprites")]
    public static void ConfigurarTudo()
    {
        int folhas = 0, tiles = 0;

        foreach (var caminho in TodosOsPngs("Assets/Art/Player", "Assets/Art/Enemies"))
        {
            if (FatiarFolha(caminho)) folhas++;
        }

        foreach (var caminho in TodosOsPngs("Assets/Art/Environment", "Assets/Art/UI"))
        {
            ConfigurarImagemSimples(caminho);
            tiles++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[Kaida] Sprites prontos: {folhas} folhas fatiadas, {tiles} imagens configuradas.");
    }

    static List<string> TodosOsPngs(params string[] pastas)
    {
        var lista = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", pastas))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.EndsWith(".png")) lista.Add(p);
        }
        lista.Sort();
        return lista;
    }

    /// <summary>Deixa a textura legível e sem compressão para podermos analisar os pixels.</summary>
    static TextureImporter PrepararImporter(string caminho)
    {
        var imp = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (imp == null) return null;

        imp.textureType = TextureImporterType.Sprite;
        imp.filterMode = FilterMode.Point;             // pixel art: nada de borrar
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.spritePixelsPerUnit = PixelsPerUnit;
        imp.mipmapEnabled = false;
        imp.isReadable = true;
        imp.wrapMode = TextureWrapMode.Clamp;
        return imp;
    }

    static void ConfigurarImagemSimples(string caminho)
    {
        var imp = PrepararImporter(caminho);
        if (imp == null) return;
        imp.spriteImportMode = SpriteImportMode.Single;

        // Fundos são desenhados lado a lado (SpriteDrawMode.Tiled). Com o mesh
        // "Tight", que é o padrão, a Unity recorta o contorno do desenho e a
        // repetição fica com falhas — aparecem faixas claras entre as cópias.
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 0;
        imp.SetTextureSettings(settings);

        imp.SaveAndReimport();
    }

    /// <summary>Fatia uma folha de animação horizontal em frames iguais.</summary>
    static bool FatiarFolha(string caminho)
    {
        var imp = PrepararImporter(caminho);
        if (imp == null) return false;

        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
        if (tex == null) return false;

        int larguraFrame = DetectarLarguraDeFrame(tex);
        if (larguraFrame <= 0)
        {
            Debug.LogWarning($"[Kaida] Não consegui detectar os frames de {caminho}; deixei como imagem única.");
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
            return false;
        }

        Vector2 pivo = CalcularPivo(tex);
        int quantidade = tex.width / larguraFrame;
        string nomeBase = System.IO.Path.GetFileNameWithoutExtension(caminho);

        // TextureImporter.spritesheet foi removido na 2022: definir aquele campo
        // compila mas não fatia nada. O caminho atual é o data provider.
        var fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        var provider = fabrica.GetSpriteEditorDataProviderFromObject(imp);
        provider.InitSpriteEditorDataProvider();

        var retangulos = new SpriteRect[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            retangulos[i] = new SpriteRect
            {
                name = $"{nomeBase}_{i}",
                spriteID = GUID.Generate(),
                rect = new Rect(i * larguraFrame, 0, larguraFrame, tex.height),
                alignment = SpriteAlignment.Custom,
                pivot = pivo
            };
        }
        provider.SetSpriteRects(retangulos);

        // a tabela nome→id precisa acompanhar, senão as referências dos clipes
        // de animação se perdem no próximo reimport
        var tabela = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (tabela != null)
        {
            var pares = retangulos
                .Select(r => new SpriteNameFileIdPair(r.name, r.spriteID))
                .ToList();
            tabela.SetNameFileIdPairs(pares);
        }

        provider.Apply();

        imp.isReadable = false;   // não precisa mais; economiza memória no build
        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();

        Debug.Log($"[Kaida] {nomeBase}: {quantidade} frames de {larguraFrame}x{tex.height}, pivô {pivo}");
        return true;
    }

    /// <summary>
    /// Conta grupos contínuos de colunas com pixels opacos. Cada grupo é um
    /// frame. Só aceita o resultado se a largura dividir certinho e cada
    /// desenho couber dentro da sua célula.
    /// </summary>
    static int DetectarLarguraDeFrame(Texture2D tex)
    {
        var pixels = tex.GetPixels32();
        int w = tex.width, h = tex.height;

        var temConteudo = new bool[w];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (pixels[y * w + x].a != 0) { temConteudo[x] = true; break; }
            }
        }

        var ilhas = new List<Vector2Int>();
        int inicio = -1;
        for (int x = 0; x < w; x++)
        {
            if (temConteudo[x] && inicio < 0) inicio = x;
            else if (!temConteudo[x] && inicio >= 0) { ilhas.Add(new Vector2Int(inicio, x - 1)); inicio = -1; }
        }
        if (inicio >= 0) ilhas.Add(new Vector2Int(inicio, w - 1));

        if (ilhas.Count == 0) return 0;

        // caso simples: uma ilha por frame
        if (w % ilhas.Count == 0 && CabeNasCelulas(ilhas, w / ilhas.Count))
            return w / ilhas.Count;

        // Partes soltas do desenho (o rabo do javali, por exemplo) viram ilhas
        // extras e a contagem acima não fecha. Aqui testa larguras padrão e
        // fica com a MENOR que encaixa: a maior também "encaixaria", mas
        // juntando dois personagens dentro do mesmo frame.
        int[] candidatos = { 16, 24, 32, 48, 64, 80, 96, 112, 128 };
        foreach (int fw in candidatos)
        {
            if (fw >= w || w % fw != 0) continue;
            if (CabeNasCelulas(ilhas, fw)) return fw;
        }
        return 0;
    }

    /// <summary>Nenhum desenho pode atravessar a divisa entre duas células.</summary>
    static bool CabeNasCelulas(List<Vector2Int> ilhas, int larguraFrame)
    {
        foreach (var ilha in ilhas)
            if (ilha.x / larguraFrame != ilha.y / larguraFrame) return false;
        return true;
    }

    /// <summary>
    /// Pivô no centro horizontal, na linha do pixel mais baixo com conteúdo.
    /// É isso que mantém os pés no chão ao trocar de animação.
    /// </summary>
    static Vector2 CalcularPivo(Texture2D tex)
    {
        var pixels = tex.GetPixels32();
        int w = tex.width, h = tex.height;

        // GetPixels32 vem de baixo para cima: y=0 é a base da imagem
        int menorY = -1;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a != 0) { menorY = y; break; }
            }
            if (menorY >= 0) break;
        }
        if (menorY < 0) return new Vector2(0.5f, 0f);
        return new Vector2(0.5f, (float)menorY / h);
    }
}
