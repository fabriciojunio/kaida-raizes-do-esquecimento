using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Recorta dois tiles do tileset da floresta e cria os assets de Tile.
///
/// O Tiles.png do pacote não é uma grade uniforme - são peças de tamanhos
/// variados (troncos, pontes, água). Só o bloco de terra do canto superior
/// esquerdo é grade de 16px, e é dele que saem o topo com grama e o miolo.
/// As regiões se diferenciam pelo tint do Tilemap, não por tilesets
/// diferentes: mantém a paleta coerente e é o que o GDD pede para o Santuário.
/// </summary>
public static class TileSetup
{
    const string PastaTiles = "Assets/Art/Tilesets";
    const string Folha = "Assets/Art/Environment/Forest/Tiles.png";

    public const string CaminhoTopo = PastaTiles + "/TileTopo.asset";
    public const string CaminhoMiolo = PastaTiles + "/TileMiolo.asset";
    public const string CaminhoAguaTopo = PastaTiles + "/TileAguaTopo.asset";
    public const string CaminhoAguaFundo = PastaTiles + "/TileAguaFundo.asset";

    [MenuItem("Kaida/4. Gerar tiles")]
    public static void GerarTiles()
    {
        PrefabBuilder.Pasta(PastaTiles);

        // célula (coluna, linha) contada de cima, em blocos de 16px
        var topo = CriarTile("TileTopo", 2, 1, CaminhoTopo);
        var miolo = CriarTile("TileMiolo", 2, 2, CaminhoMiolo);

        // A água fica numa camada sem colisão: atravessa-se nadando de olho,
        // não é chão nem parede.
        var aguaTopo = CriarTile("TileAguaTopo", 6, 18, CaminhoAguaTopo, Tile.ColliderType.None);
        var aguaFundo = CriarTile("TileAguaFundo", 3, 19, CaminhoAguaFundo, Tile.ColliderType.None);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Kaida] Tiles criados: chão {(topo != null && miolo != null)}, " +
                  $"água {(aguaTopo != null && aguaFundo != null)}");
    }

    static Tile CriarTile(string nome, int coluna, int linha, string destino,
                          Tile.ColliderType colisao = Tile.ColliderType.Grid)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Folha);
        if (tex == null)
        {
            Debug.LogError("[Kaida] Tiles.png não encontrado em " + Folha);
            return null;
        }

        // um sprite recortado da folha; o eixo Y do Unity sobe, o da imagem desce
        int y = tex.height - (linha + 1) * 16;
        var sprite = Sprite.Create(tex, new Rect(coluna * 16, y, 16, 16),
                                   new Vector2(0.5f, 0.5f), SpriteSheetSetup.PixelsPerUnit);
        sprite.name = nome + "_sprite";

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = nome;
        tile.sprite = sprite;
        tile.colliderType = colisao;

        AssetDatabase.CreateAsset(tile, destino);
        AssetDatabase.AddObjectToAsset(sprite, tile);
        EditorUtility.SetDirty(tile);
        return tile;
    }
}
