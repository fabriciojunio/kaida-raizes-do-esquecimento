using UnityEditor;
using UnityEngine;

/// <summary>
/// Recorta peças isoladas das folhas de cenário e salva cada uma como sprite
/// próprio: itens, vegetação e construções.
///
/// Existe porque usar a folha inteira como sprite de um objeto coloca no
/// mundo uma imagem de dezenas de unidades - a tela fica coberta de pedras e
/// runas soltas. Cada peça precisa da sua janela, medida nos pixels.
/// </summary>
public static class RecorteDeSprites
{
    const string Pasta = "Assets/Art/Itens";
    const string Env = "Assets/Art/Environment/Forest/";

    // itens de gameplay
    public const string Tocha       = Pasta + "/Tocha.asset";
    public const string Chave       = Pasta + "/Chave.asset";
    public const string Medalhao    = Pasta + "/Medalhao.asset";
    public const string FrascoVida  = Pasta + "/FrascoVida.asset";
    public const string FrascoLumen = Pasta + "/FrascoLumen.asset";

    // vegetação: uma cor de árvore por região
    public const string ArvoreVerde   = Pasta + "/ArvoreVerde.asset";
    public const string ArvoreEscura  = Pasta + "/ArvoreEscura.asset";
    public const string ArvoreDourada = Pasta + "/ArvoreDourada.asset";
    public const string ArvoreVermelha= Pasta + "/ArvoreVermelha.asset";
    public const string ArvoreBaixa   = Pasta + "/ArvoreBaixa.asset";

    // arbustos e miudezas de chão
    public const string Copa1     = Pasta + "/Copa1.asset";
    public const string Copa2     = Pasta + "/Copa2.asset";
    public const string Copa3     = Pasta + "/Copa3.asset";
    public const string Copa4     = Pasta + "/Copa4.asset";
    public const string Cogumelos = Pasta + "/Cogumelos.asset";

    // construções da vila
    public const string Casa  = Pasta + "/Casa.asset";
    public const string Porta = Pasta + "/Porta.asset";

    /// <summary>Uma janela numa folha, medida de cima para baixo como na imagem.</summary>
    struct Peca
    {
        public string nome, destino, folha;
        public int x, yDoTopo, largura, altura;
        public Vector2 pivo;

        public Peca(string nome, string destino, string folha,
                    int x, int yDoTopo, int largura, int altura, Vector2? pivo = null)
        {
            this.nome = nome; this.destino = destino; this.folha = folha;
            this.x = x; this.yDoTopo = yDoTopo;
            this.largura = largura; this.altura = altura;
            this.pivo = pivo ?? new Vector2(0.5f, 0f);   // apoiado no chão
        }
    }

    [MenuItem("Kaida/4b. Recortar itens e vegetação", false, 12)]
    public static void RecortarTudo()
    {
        PrefabBuilder.Pasta(Pasta);

        var pecas = new[]
        {
            // --- itens de gameplay (folha Tiles.png) ---
            new Peca("Tocha",       Tocha,       Env + "Tiles.png", 336, 240, 16, 32),
            new Peca("Chave",       Chave,       Env + "Tiles.png", 240, 319, 24, 13),
            new Peca("Medalhao",    Medalhao,    Env + "Tiles.png", 240, 335, 16, 16),
            new Peca("FrascoVida",  FrascoVida,  Env + "Tiles.png", 240, 352, 16, 16),
            new Peca("FrascoLumen", FrascoLumen, Env + "Tiles.png", 304, 352, 16, 16),

            // --- árvores inteiras, uma cor por região ---
            new Peca("ArvoreVerde",    ArvoreVerde,    Env + "Trees/Green-Tree.png",  0, 0, 107, 368),
            new Peca("ArvoreEscura",   ArvoreEscura,   Env + "Trees/Dark-Tree.png",   0, 0, 107, 368),
            new Peca("ArvoreDourada",  ArvoreDourada,  Env + "Trees/Golden-Tree.png", 0, 0, 107, 368),
            new Peca("ArvoreVermelha", ArvoreVermelha, Env + "Trees/Red-Tree.png",    0, 0, 107, 368),
            new Peca("ArvoreBaixa",    ArvoreBaixa,    Env + "Trees/Green-Tree.png",  2, 391, 108, 313),

            // --- arbustos (copas soltas) e cogumelos ---
            new Peca("Copa1",     Copa1,     Env + "Tree-Assets.png", 210,   5, 124, 86),
            new Peca("Copa2",     Copa2,     Env + "Tree-Assets.png", 210, 101, 124, 86),
            new Peca("Copa3",     Copa3,     Env + "Tree-Assets.png", 210, 197, 124, 86),
            new Peca("Copa4",     Copa4,     Env + "Tree-Assets.png", 210, 293, 124, 86),
            new Peca("Cogumelos", Cogumelos, Env + "Tree-Assets.png", 129,   0,  62, 32),

            // --- construções da Orla da Vila ---
            new Peca("Casa",  Casa,  Env + "Buildings.png", 238,  12, 76, 86),
            new Peca("Porta", Porta, Env + "Buildings.png", 341, 122, 38, 54),
        };

        int feitos = 0;
        foreach (var p in pecas) if (Criar(p)) feitos++;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Kaida] {feitos} de {pecas.Length} peças recortadas.");
    }

    static bool Criar(Peca p)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p.folha);
        if (tex == null)
        {
            Debug.LogWarning($"[Kaida] folha não encontrada: {p.folha}");
            return false;
        }

        // a folha mede de cima para baixo; o sprite mede de baixo para cima
        int y = tex.height - (p.yDoTopo + p.altura);
        if (y < 0 || p.x + p.largura > tex.width || p.yDoTopo + p.altura > tex.height)
        {
            Debug.LogWarning($"[Kaida] recorte de {p.nome} cai fora de {p.folha}");
            return false;
        }

        var sprite = Sprite.Create(tex, new Rect(p.x, y, p.largura, p.altura),
                                   p.pivo, SpriteSheetSetup.PixelsPerUnit);
        sprite.name = p.nome;

        var recipiente = ScriptableObject.CreateInstance<SpriteRecortado>();
        recipiente.name = p.nome;
        recipiente.sprite = sprite;

        AssetDatabase.CreateAsset(recipiente, p.destino);
        AssetDatabase.AddObjectToAsset(sprite, recipiente);
        EditorUtility.SetDirty(recipiente);
        return true;
    }

    public static Sprite Carregar(string caminho)
    {
        var r = AssetDatabase.LoadAssetAtPath<SpriteRecortado>(caminho);
        return r != null ? r.sprite : null;
    }
}
