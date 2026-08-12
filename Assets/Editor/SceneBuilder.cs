using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constrói as quatro regiões do vale a partir de mapas em texto.
///
/// O level design fica legível e editável aqui em cima: cada caractere é um
/// tile de 1 unidade. Mexer no mapa e rodar de novo regenera a cena, o que
/// é bem mais rápido do que arrastar objetos na Scene View.
///
/// Legenda:
///   #  chão sólido          =  plataforma
///   P  início da região     p  chegada vindo da direita
///   C  marco de descanso    X  perigo (espinhos / esporos)
///   B  javali-casca         A  abelha-eco        S  caracol-rastejante
///   H  habilidade           F  fragmento de lúmen  N  nódulo de vida
///   <  volta para a região anterior   >  segue para a próxima
///   G  o Guardião do Lúmen
/// </summary>
public static class SceneBuilder
{
    const string PastaCenas = "Assets/Scenes";

    // Distâncias de referência com os stats atuais:
    //   pulo simples sobe 4 tiles e cruza ~5 na horizontal
    //   dash cruza ~3,2 tiles
    // Nenhum salto obrigatório aqui passa disso.

    static readonly string[] OrlaDaVila = {
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        ".....................N..........................................",
        "..................=======.......................................",
        "................................................................",
        "................................................................",
        "..............=======.........=======...........................",
        "................................................................",
        ".............F..................................................",
        "..........=======.........=======...........=======.............",
        "................................................................",
        "..............P....C..................B.............B........>..",
        "################################################################",
        "################################################################",
        "################################################################",
    };

    static readonly string[] FlorestaSilente = {
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "...................................H............................",
        "................................=======.........................",
        "................................................................",
        "................................................................",
        "............................=======.............................",
        "................................................................",
        "...........................S....................................",
        "........................=======.............=======.............",
        "................................................................",
        ".......................F........................................",
        "....................=======.............=======.................",
        "................................................................",
        ".<............p...C...............B.................B...C....>..",
        "################################################################",
        "################################################################",
        "################################################################",
    };

    // Lago Silente: a travessia é por cima da água, saltando entre
    // plataformas. Cair não mata - só devolve o caminho já andado.
    static readonly string[] LagoSilente = {
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        ".....................................A..........................",
        "..................................=======.......................",
        "................................................................",
        ".................................................N..............",
        "..........................=======.............=======...........",
        "................................................................",
        "........................F.......................................",
        "......................=====...=====...=====.....................",
        "................................................................",
        ".<............p..C................................C.....B....>..",
        "####################~~~~~~~~~~~~~~~~~~~~~~~~~~##################",
        "####################::::::::::::::::::::::::::##################",
        "################################################################",
    };

    // Caverna Musgosa: a região da escalada de parede.
    //
    // A saída fica no alto de um poço fechado nas colunas 55-56 e 60-63, com
    // três unidades de vão e um descanso na metade da subida. Andar
    // reto até a passagem não funciona mais: quem chega ao pé do poço só sobe
    // saltando de uma parede para a outra, e para isso precisa ter pegado o
    // 'H' lá atrás. Antes as cinco regiões eram atravessáveis pelo chão do
    // começo ao fim, e as duas habilidades nunca chegavam a ser cobradas.
    static readonly string[] CavernaMusgosa = {
        "................................................................",
        "................................................................",
        "................................................................",
        "..............................................................>.",
        "...........................H...........................##...####",
        "........................=======........................##...####",
        ".......................................................##...####",
        ".......................N...............................##...####",
        "....................=======...........=======..........##...####",
        ".......................................................##...####",
        ".....................................A.................##==.####",
        "................=======...........=======..............##...####",
        ".......................................................##...####",
        ".................................S.....................##...####",
        "............=======...........=======...........=======.....####",
        "............................................................####",
        ".<............p...C.....................S.....A....C........####",
        "################################################################",
        "################################################################",
        "################################################################",
    };

    // Arena do chefe. As plataformas são escadas dos dois lados: o Guardião
    // flutua na altura da plataforma do meio, então dá para alcançá-lo nas
    // fases 1 e 2 sem precisar do pulo duplo - que ainda assim ajuda muito.
    static readonly string[] SantuarioEsquecido = {
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................................................",
        "................................G...............................",
        "............................=========...........................",
        "................................................................",
        "................................................................",
        "....................=======...........=======...................",
        "................................................................",
        "................................................................",
        "..........=======...............................=======.........",
        "................................................................",
        ".<............p...............C.................................",
        "################################################################",
        "################################################################",
        "################################################################",
    };

    struct Regiao
    {
        public string arquivo;
        public string[] mapa;
        public Color tint;
        public string proxima, anterior;
        public string fundo;

        /// <summary>Árvore que caracteriza a região (cada uma tem a sua cor).</summary>
        public string arvore;
        /// <summary>Quantas árvores espalhar no fundo.</summary>
        public int densidadeDeArvores;
        /// <summary>Casas na borda do cenário - só a Vila tem.</summary>
        public bool temCasas;
        /// <summary>Arbustos e cogumelos em cima do chão.</summary>
        public bool temVegetacaoDeChao;
        /// <summary>Quedas d'água caindo no lago.</summary>
        public bool temCachoeiras;

        /// <summary>
        /// Poço vertical que só se vence saltando de parede em parede. Fica
        /// declarado aqui, e não deduzido do mapa, porque é uma decisão de
        /// level design: é o trecho que cobra a habilidade.
        /// Rect vazio = a região não tem poço.
        /// </summary>
        public Rect pocoDeEscalada;
    }

    [MenuItem("Kaida/5. Gerar cenas")]
    public static void GerarTudo()
    {
        PrefabBuilder.Pasta(PastaCenas);

        var regioes = new[]
        {
            // Cada região tem cor de árvore, tint e densidade próprias - é o
            // que faz o vale parecer quatro lugares e não o mesmo mapa repetido.
            new Regiao {
                arquivo = "01_OrlaDaVila", mapa = OrlaDaVila,
                tint = Color.white,
                proxima = "02_FlorestaSilente", anterior = "",
                fundo = "Assets/Art/Environment/Forest/Sky.png",
                arvore = RecorteDeSprites.ArvoreVerde,
                densidadeDeArvores = 5, temCasas = true, temVegetacaoDeChao = true
            },
            new Regiao {
                arquivo = "02_FlorestaSilente", mapa = FlorestaSilente,
                tint = new Color(0.80f, 0.90f, 0.78f),      // verde silencioso
                proxima = "03_LagoSilente", anterior = "01_OrlaDaVila",
                fundo = "Assets/Art/Environment/Forest/Sky.png",
                arvore = RecorteDeSprites.ArvoreEscura,
                densidadeDeArvores = 11, temCasas = false, temVegetacaoDeChao = true
            },
            new Regiao {
                arquivo = "03_LagoSilente", mapa = LagoSilente,
                tint = new Color(0.86f, 0.94f, 0.98f),      // luz refletida na água
                proxima = "04_CavernaMusgosa", anterior = "02_FlorestaSilente",
                fundo = "Assets/Art/Environment/Forest/Sky.png",
                arvore = RecorteDeSprites.ArvoreDourada,
                densidadeDeArvores = 7, temCasas = false, temVegetacaoDeChao = true,
                temCachoeiras = true
            },
            new Regiao {
                arquivo = "04_CavernaMusgosa", mapa = CavernaMusgosa,
                tint = new Color(0.55f, 0.68f, 0.80f),      // azul-acinzentado, úmido
                proxima = "05_SantuarioEsquecido", anterior = "03_LagoSilente",
                fundo = "Assets/Art/Environment/Cavern/CavernBg1.png",
                arvore = "",                                 // não crescem árvores lá embaixo
                densidadeDeArvores = 0, temCasas = false, temVegetacaoDeChao = true,
                // paredes nas colunas 55-56 e 60-63; o topo inclui a passagem, em y=17,5
                pocoDeEscalada = new Rect(54.5f, 2.5f, 10f, 15.5f)
            },
            new Regiao {
                arquivo = "05_SantuarioEsquecido", mapa = SantuarioEsquecido,
                tint = new Color(0.62f, 0.48f, 0.72f),      // a caverna corrompida
                proxima = "", anterior = "04_CavernaMusgosa",
                fundo = "Assets/Art/Environment/Cavern/CavernBg2.png",
                arvore = RecorteDeSprites.ArvoreVermelha,    // o vale corrompido
                densidadeDeArvores = 6, temCasas = false, temVegetacaoDeChao = false
            },
        };

        var caminhos = new List<string> { ConstruirMenu() };
        foreach (var r in regioes) caminhos.Add(Construir(r));

        // Build Settings na ordem de progressão: o menu tem que ser a primeira,
        // porque é a cena que o executável abre
        var lista = new List<EditorBuildSettingsScene>();
        foreach (var c in caminhos) lista.Add(new EditorBuildSettingsScene(c, true));
        EditorBuildSettings.scenes = lista.ToArray();

        AssetDatabase.SaveAssets();
        Debug.Log($"[Kaida] {caminhos.Count} cenas geradas e registradas em Build Settings.");
    }

    /// <summary>
    /// Tela inicial. Em vez de um fundo chapado, monta um pedaço real de
    /// cenário com a Kaida andando sozinha; a câmera renderiza para uma
    /// textura que aparece desfocada atrás do menu.
    /// </summary>
    static string ConstruirMenu()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // um trecho curto de floresta, só para ter o que olhar
        var vitrine = new Regiao
        {
            arquivo = "00_MenuPrincipal",
            mapa = CenarioDoMenu,
            tint = new Color(0.86f, 0.94f, 0.9f),
            fundo = "Assets/Art/Environment/Forest/Sky.png",
            arvore = RecorteDeSprites.ArvoreVerde,
            densidadeDeArvores = 6,
            temCasas = true,
            temVegetacaoDeChao = true,
            temCachoeiras = false
        };

        int largura = CenarioDoMenu[0].Length;
        int altura = CenarioDoMenu.Length;

        MontarFundo(vitrine, largura, altura);
        MontarTilemap(vitrine, largura, altura);
        MontarDecoracao(vitrine, largura, altura);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        camGO.transform.position = new Vector3(largura * 0.5f, altura * 0.42f, -10f);
        camGO.AddComponent<AudioListener>();

        // a Kaida de vitrine: só aparência, sem física nem controle
        var prefabJogador = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kaida.prefab");
        if (prefabJogador != null)
        {
            var kaida = (GameObject)PrefabUtility.InstantiatePrefab(prefabJogador);
            PrefabUtility.UnpackPrefabInstance(kaida, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            kaida.name = "KaidaDeVitrine";

            // fora tudo que é jogabilidade
            Object.DestroyImmediate(kaida.GetComponent<PlayerController>());
            Object.DestroyImmediate(kaida.GetComponent<Rigidbody2D>());
            foreach (var c in kaida.GetComponentsInChildren<Collider2D>()) Object.DestroyImmediate(c);

            // apoiada no piso do cenário, não numa altura chutada: com y fixo
            // ela ficava pairando um tile acima da grama
            float piso = AlturaDoChao(vitrine, largura, altura, (int)(largura * 0.62f));
            kaida.transform.position = new Vector3(largura * 0.62f, piso, 0f);

            var vitrineScript = kaida.AddComponent<KaidaDeVitrine>();
            vitrineScript.animator = kaida.GetComponent<Animator>();
            vitrineScript.spriteRenderer = kaida.GetComponent<SpriteRenderer>();
            vitrineScript.alcance = 6f;
        }

        var sistemas = new GameObject("_Sistemas");
        sistemas.AddComponent<GameManager>();
        sistemas.AddComponent<SaveSystem>();
        AdicionarTrilha(sistemas, "00_MenuPrincipal");
        // Em objeto separado, sem DontDestroyOnLoad: o _Sistemas do menu
        // sobrevive à troca de cena e o da região seguinte é descartado por
        // ser duplicata. Junto, o cursor do menu continuaria valendo no jogo.
        var cursorMenu = new GameObject("Cursor");
        cursorMenu.AddComponent<ControleDoCursor>().mostrarCursor = true;

        // desfoque primeiro (fica atrás), menu depois
        var fundo = new GameObject("FundoDesfocado");
        var desfoque = fundo.AddComponent<FundoDesfocado>();
        desfoque.cameraDoCenario = cam;
        desfoque.raio = 5.5f;
        desfoque.escurecer = 0.34f;

        // Tela de créditos: exigência da entrega, que pede a fonte dos assets
        // dentro do próprio jogo, não só no relatório.
        var creditosGO = new GameObject("Creditos");
        var creditos = creditosGO.AddComponent<CreditosUI>();
        creditos.equipe = EquipeDoProjeto;

        var menu = new GameObject("MenuPrincipal");
        var mainMenu = menu.AddComponent<MainMenu>();
        mainMenu.creditos = creditos;

        string caminho = $"{PastaCenas}/00_MenuPrincipal.unity";
        EditorSceneManager.SaveScene(cena, caminho);
        Debug.Log("[Kaida] Cena montada: 00_MenuPrincipal (com cenário ao fundo)");
        return caminho;
    }

    /// <summary>
    /// Integrantes do grupo, como aparecem nos créditos do jogo e no README.
    /// Um nome por linha.
    /// </summary>
    public const string EquipeDoProjeto =
        "Fabrício Júnio Almeida Dias\n" +
        "Camila Pereira Raimundo\n" +
        "Luan Miranda Padilha\n" +
        "Kauã Limão Nunes";

    /// <summary>
    /// Cenário curto que fica rodando atrás do menu.
    ///
    /// Sem plataformas soltas: aqui não se joga, então um pedaço de terra
    /// pairando no meio do nada não é level design, é sujeira na tela - e era
    /// exatamente assim que aparecia atrás do desfoque.
    /// </summary>
    static readonly string[] CenarioDoMenu = {
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "..............................",
        "##############################",
        "##############################",
    };

    /// <summary>
    /// Trilha da região. Cada uma recebe tônica e andamento próprios: a Vila
    /// é mais clara e ligeira, o Santuário grave e arrastado.
    /// </summary>
    static void AdicionarTrilha(GameObject alvo, string regiao)
    {
        var fonte = alvo.AddComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.loop = true;

        var trilha = alvo.AddComponent<TrilhaSonora>();

        // Volumes baixos de propósito: é trilha de fundo, para dar presença ao
        // lugar sem disputar atenção com o jogo. Notas longas reforçam isso -
        // quanto mais lento o arpejo, menos ele soa como "música tocando" e
        // mais como o ambiente do vale.
        if (regiao.StartsWith("00_"))      { trilha.tonica = 220.00f; trilha.duracaoDaNota = 1.15f; trilha.volume = 0.085f; }
        else if (regiao.StartsWith("01_")) { trilha.tonica = 246.94f; trilha.duracaoDaNota = 1.05f; trilha.volume = 0.060f; }
        else if (regiao.StartsWith("02_")) { trilha.tonica = 196.00f; trilha.duracaoDaNota = 1.20f; trilha.volume = 0.060f; }
        else if (regiao.StartsWith("03_")) { trilha.tonica = 261.63f; trilha.duracaoDaNota = 1.10f; trilha.volume = 0.055f; }
        else if (regiao.StartsWith("04_")) { trilha.tonica = 174.61f; trilha.duracaoDaNota = 1.40f; trilha.volume = 0.070f; }
        else                               { trilha.tonica = 146.83f; trilha.duracaoDaNota = 1.55f; trilha.volume = 0.080f; }

        trilha.compassos = 8;
    }

    static string Construir(Regiao r)
    {
        ValidarMapa(r.arquivo, r.mapa);

        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int altura = r.mapa.Length;
        int largura = r.mapa[0].Length;

        MontarFundo(r, largura, altura);
        var tilemap = MontarTilemap(r, largura, altura);
        MontarDecoracao(r, largura, altura);
        var player = MontarSistemasEJogador(r, largura, altura);
        PovoarMapa(r, largura, altura, player);

        string caminho = $"{PastaCenas}/{r.arquivo}.unity";
        EditorSceneManager.SaveScene(cena, caminho);
        Debug.Log($"[Kaida] Cena montada: {r.arquivo} ({largura}x{altura} tiles)");
        return caminho;
    }

    /// <summary>Uma linha mais curta desalinharia o mapa inteiro sem avisar.</summary>
    static void ValidarMapa(string nome, string[] mapa)
    {
        int largura = mapa[0].Length;
        for (int i = 0; i < mapa.Length; i++)
        {
            if (mapa[i].Length != largura)
                throw new System.Exception(
                    $"[Kaida] Mapa '{nome}': linha {i} tem {mapa[i].Length} colunas, esperado {largura}.");
        }
    }

    // --------------------------------------------------------------- tilemap
    static Tilemap MontarTilemap(Regiao r, int largura, int altura)
    {
        var gridGO = new GameObject("Grid");
        var grid = gridGO.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        var tmGO = new GameObject("Ground");
        tmGO.transform.SetParent(gridGO.transform, false);
        tmGO.layer = PrefabBuilder.LayerGround;

        var tilemap = tmGO.AddComponent<Tilemap>();
        var renderer = tmGO.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;
        tilemap.color = r.tint;

        // Os tiles vêm ANTES dos colisores. O TilemapCollider2D monta a forma
        // a partir do que existe no tilemap no momento em que é criado; num
        // tilemap ainda vazio ele nasce sem nada e não se recompõe sozinho -
        // o chão fica atravessável e nada na tela denuncia.
        var topo = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoTopo);
        var miolo = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoMiolo);
        if (topo == null || miolo == null)
            Debug.LogError("[Kaida] Tiles não encontrados - rode 'Kaida/4. Gerar tiles' antes.");

        for (int linha = 0; linha < altura; linha++)
        {
            for (int coluna = 0; coluna < largura; coluna++)
            {
                // as plataformas '=' vão para outra camada, atravessável
                char c = r.mapa[linha][coluna];
                if (c != '#') continue;

                // usa o tile com grama quando não há nada sólido logo acima
                bool descoberto = linha == 0 || r.mapa[linha - 1][coluna] != '#';
                var tile = descoberto ? topo : miolo;
                tilemap.SetTile(new Vector3Int(coluna, altura - 1 - linha, 0), tile);
            }
        }

        tilemap.RefreshAllTiles();
        tilemap.CompressBounds();

        MontarPlataformas(r, gridGO, largura, altura);

        // agora sim os colisores, sobre um mapa já preenchido
        var col = tmGO.AddComponent<TilemapCollider2D>();

        var composite = tmGO.AddComponent<CompositeCollider2D>();   // já traz o Rigidbody2D
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;

        var rb = tmGO.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // `usedByComposite` só pega depois que o CompositeCollider2D existe
        col.usedByComposite = true;
        composite.GenerateGeometry();

        // Rede de segurança: em tempo de execução a geometria é reconstruída
        // no Awake. Se algo na serialização da cena vier vazio, o jogo se
        // conserta sozinho em vez de deixar o jogador cair pelo mundo.
        tmGO.AddComponent<GarantirColisaoDoChao>();

        MontarAgua(r, gridGO, largura, altura);
        return tilemap;
    }

    /// <summary>
    /// Plataformas atravessáveis: sobe-se por baixo e pousa-se em cima.
    ///
    /// Ficam numa camada própria com PlatformEffector2D. Sem isso, uma
    /// plataforma no meio do caminho vira parede: o jogador bate a cabeça e
    /// não consegue passar, o que travava a travessia em várias regiões.
    /// </summary>
    static void MontarPlataformas(Regiao r, GameObject grid, int largura, int altura)
    {
        bool tem = false;
        foreach (var l in r.mapa) if (l.IndexOf('=') >= 0) { tem = true; break; }
        if (!tem) return;

        var topo = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoTopo);
        if (topo == null) return;

        var go = new GameObject("Plataformas");
        go.transform.SetParent(grid.transform, false);
        go.layer = PrefabBuilder.LayerGround;

        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;
        tilemap.color = r.tint;

        for (int linha = 0; linha < altura; linha++)
            for (int coluna = 0; coluna < largura; coluna++)
                if (r.mapa[linha][coluna] == '=')
                    tilemap.SetTile(new Vector3Int(coluna, altura - 1 - linha, 0), topo);

        tilemap.RefreshAllTiles();
        tilemap.CompressBounds();

        var col = go.AddComponent<TilemapCollider2D>();
        var composto = go.AddComponent<CompositeCollider2D>();
        composto.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composto.generationType = CompositeCollider2D.GenerationType.Synchronous;

        var rb = go.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        col.usedByComposite = true;
        composto.usedByEffector = true;
        composto.GenerateGeometry();

        // só bloqueia quem vem de cima; por baixo e pelos lados, passa
        var efeito = go.AddComponent<PlatformEffector2D>();
        efeito.useOneWay = true;
        efeito.useOneWayGrouping = true;
        efeito.surfaceArc = 150f;
        efeito.useSideFriction = false;
        efeito.useSideBounce = false;

        go.AddComponent<GarantirColisaoDoChao>();
    }

    /// <summary>
    /// Camada de água, sem colisão e atrás do chão. Fica num tilemap próprio
    /// para não entrar na geometria de colisão do cenário.
    /// </summary>
    static void MontarAgua(Regiao r, GameObject grid, int largura, int altura)
    {
        var aguaTopo = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoAguaTopo);
        var aguaFundo = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoAguaFundo);
        if (aguaTopo == null || aguaFundo == null) return;

        bool temAgua = false;
        foreach (var l in r.mapa)
            if (l.IndexOf('~') >= 0 || l.IndexOf(':') >= 0) { temAgua = true; break; }
        if (!temAgua) return;

        var go = new GameObject("Agua");
        go.transform.SetParent(grid.transform, false);

        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10;                 // atrás do chão, à frente do fundo
        tilemap.color = new Color(1f, 1f, 1f, 0.9f);

        for (int linha = 0; linha < altura; linha++)
        {
            for (int coluna = 0; coluna < largura; coluna++)
            {
                char c = r.mapa[linha][coluna];
                if (c != '~' && c != ':') continue;
                tilemap.SetTile(new Vector3Int(coluna, altura - 1 - linha, 0),
                                c == '~' ? aguaTopo : aguaFundo);
            }
        }
        tilemap.RefreshAllTiles();

        // Afogamento: cair no lago custa vida e devolve ao último marco. Sem
        // isso dava para andar dentro d'água como se fosse chão pintado, e a
        // travessia por plataformas perdia completamente o sentido.
        for (int linha = 0; linha < altura; linha++)
        {
            int inicio = -1;
            for (int coluna = 0; coluna <= largura; coluna++)
            {
                bool ehAgua = coluna < largura && r.mapa[linha][coluna] == '~';
                if (ehAgua && inicio < 0) inicio = coluna;
                else if (!ehAgua && inicio >= 0)
                {
                    CriarFaixaDeAfogamento(go.transform, inicio, coluna - 1, altura - 1 - linha);
                    inicio = -1;
                }
            }
        }
    }

    /// <summary>Um trigger por trecho contínuo de água, em vez de um por tile.</summary>
    static void CriarFaixaDeAfogamento(Transform pai, int colunaInicial, int colunaFinal, float y)
    {
        var go = new GameObject("Afogamento");
        go.transform.SetParent(pai, false);

        float largura = colunaFinal - colunaInicial + 1;
        go.transform.position = new Vector3(colunaInicial + largura * 0.5f, y + 0.2f, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(largura, 0.9f);

        var h = go.AddComponent<Hazard>();
        h.damage = 1;
        h.returnToCheckpoint = true;
        h.repeatInterval = 1f;
    }

    /// <summary>
    /// Espalha vegetação e construções. Usa um gerador com semente fixa: o
    /// cenário sai variado, mas igual toda vez que o projeto for remontado.
    /// </summary>
    static void MontarDecoracao(Regiao r, int largura, int altura)
    {
        var pai = new GameObject("Decoracao");
        var sorte = new System.Random(r.arquivo.GetHashCode());

        // --- árvores ao fundo, atrás de tudo ---
        var arvore = string.IsNullOrEmpty(r.arvore) ? null : RecorteDeSprites.Carregar(r.arvore);
        if (arvore != null && r.densidadeDeArvores > 0)
        {
            float passo = (float)largura / r.densidadeDeArvores;
            for (int i = 0; i < r.densidadeDeArvores; i++)
            {
                float x = passo * i + (float)sorte.NextDouble() * passo * 0.6f;
                float escala = 0.55f + (float)sorte.NextDouble() * 0.35f;

                var go = new GameObject("Arvore");
                go.transform.SetParent(pai.transform);
                go.transform.position = new Vector3(x, AlturaDoChao(r, largura, altura, (int)x) - 0.5f, 5f);
                go.transform.localScale = Vector3.one * escala;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = arvore;
                sr.sortingOrder = -30;
                // as mais distantes ficam menores e mais lavadas
                float distancia = Mathf.InverseLerp(0.55f, 0.9f, escala);
                sr.color = Color.Lerp(Color.Lerp(r.tint, Color.white, 0.45f), r.tint, distancia);
                if (sorte.Next(2) == 0) sr.flipX = true;
            }
        }

        // --- casas: só a Orla da Vila, e só no chão ---
        if (r.temCasas)
        {
            var casa = RecorteDeSprites.Carregar(RecorteDeSprites.Casa);
            var porta = RecorteDeSprites.Carregar(RecorteDeSprites.Porta);

            // Nível do piso principal. As casas só entram aqui: empoleiradas
            // em plataforma solta, no meio do ar, elas não fazem sentido
            // nenhum e ainda escondem o caminho.
            float nivelDoPiso = AlturaDoChao(r, largura, altura, 0);

            int[] colunas = { 5, 25, 44 };
            foreach (int col in colunas)
            {
                float chao = AlturaDoChao(r, largura, altura, col);
                if (chao < 0f) continue;
                if (Mathf.Abs(chao - nivelDoPiso) > 0.1f) continue;   // não é o piso

                if (casa != null)
                {
                    var go = new GameObject("Casa");
                    go.transform.SetParent(pai.transform);
                    go.transform.position = new Vector3(col, chao, 2f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = casa;
                    sr.sortingOrder = -20;
                    sr.color = Color.Lerp(r.tint, Color.white, 0.15f);
                }
                if (porta != null && sorte.Next(2) == 0)
                {
                    var go = new GameObject("Porta");
                    go.transform.SetParent(pai.transform);
                    go.transform.position = new Vector3(col + 3.5f, chao, 1f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = porta;
                    sr.sortingOrder = -18;
                }
            }
        }

        // --- cachoeiras: onde há lago, a água vem de algum lugar ---
        if (r.temCachoeiras) MontarCachoeiras(r, pai.transform, largura, altura);

        // --- arbustos e cogumelos em cima do chão ---
        if (r.temVegetacaoDeChao)
        {
            var arbustos = new[]
            {
                RecorteDeSprites.Carregar(RecorteDeSprites.Copa1),
                RecorteDeSprites.Carregar(RecorteDeSprites.Copa2),
                RecorteDeSprites.Carregar(RecorteDeSprites.Copa3),
                RecorteDeSprites.Carregar(RecorteDeSprites.Copa4),
            };
            var cogumelos = RecorteDeSprites.Carregar(RecorteDeSprites.Cogumelos);

            for (int coluna = 2; coluna < largura - 2; coluna += 3)
            {
                if (sorte.Next(100) > 45) continue;
                float chao = AlturaDoChao(r, largura, altura, coluna);
                if (chao < 0f) continue;

                bool ehCogumelo = cogumelos != null && sorte.Next(100) < 30;
                var sprite = ehCogumelo ? cogumelos : arbustos[sorte.Next(arbustos.Length)];
                if (sprite == null) continue;

                var go = new GameObject(ehCogumelo ? "Cogumelos" : "Arbusto");
                go.transform.SetParent(pai.transform);
                go.transform.position = new Vector3(coluna + (float)sorte.NextDouble(), chao - 0.15f, 1f);

                float escala = ehCogumelo ? 0.9f : 0.30f + (float)sorte.NextDouble() * 0.22f;
                go.transform.localScale = Vector3.one * escala;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = ehCogumelo ? 3 : -5;   // arbusto atrás, cogumelo à frente
                sr.color = Color.Lerp(r.tint, Color.white, 0.1f);
                if (sorte.Next(2) == 0) sr.flipX = true;
            }
        }
    }

    /// <summary>
    /// Colunas de água caindo do alto até a superfície do lago. Ficam atrás do
    /// chão, então passam por trás das plataformas em vez de cobri-las.
    /// </summary>
    static void MontarCachoeiras(Regiao r, Transform pai, int largura, int altura)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoAguaFundo);
        var espuma = AssetDatabase.LoadAssetAtPath<Tile>(TileSetup.CaminhoAguaTopo);
        if (tile == null || tile.sprite == null) return;

        // acha o nível da superfície do lago no mapa
        int linhaDaAgua = -1;
        for (int linha = 0; linha < altura; linha++)
            if (r.mapa[linha].IndexOf('~') >= 0) { linhaDaAgua = linha; break; }
        if (linhaDaAgua < 0) return;

        float nivel = altura - 1 - linhaDaAgua;
        int[] colunas = { 21, 33, 48 };

        foreach (int col in colunas)
        {
            if (col < 0 || col >= largura) continue;

            var queda = new GameObject($"Cachoeira_{col}");
            queda.transform.SetParent(pai, false);

            for (float y = nivel; y < altura; y += 1f)
            {
                bool noTopo = y > altura - 2f;
                var go = new GameObject("Agua");
                go.transform.SetParent(queda.transform, false);
                go.transform.position = new Vector3(col + 0.5f, y + 0.5f, 3f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = (noTopo && espuma != null) ? espuma.sprite : tile.sprite;
                sr.sortingOrder = -8;                       // atrás do chão
                sr.color = new Color(1f, 1f, 1f, 0.82f);
            }

            // espuma na base, onde a queda encontra o lago
            if (espuma != null)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    var go = new GameObject("Espuma");
                    go.transform.SetParent(queda.transform, false);
                    go.transform.position = new Vector3(col + 0.5f + dx, nivel + 0.5f, 2f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = espuma.sprite;
                    sr.sortingOrder = 2;
                    sr.color = new Color(1f, 1f, 1f, 0.6f);
                }
            }
        }
    }

    /// <summary>Altura do topo do chão numa coluna, ou -1 se não houver chão.</summary>
    static float AlturaDoChao(Regiao r, int largura, int altura, int coluna)
    {
        if (coluna < 0 || coluna >= largura) return -1f;
        for (int linha = 0; linha < altura; linha++)
        {
            char c = r.mapa[linha][coluna];
            if (c == '#' || c == '=') return altura - linha;   // topo da célula
        }
        return -1f;
    }

    // ----------------------------------------------------------------- fundo
    /// <summary>
    /// Fundo em três camadas, sem deixar céu aparecendo.
    ///
    /// As cópias são instanciadas lado a lado em vez de usar
    /// SpriteDrawMode.Tiled: esticar um sprite deixa emendas visíveis como
    /// faixas claras, e nenhum ajuste de import resolve isso direito.
    /// </summary>
    static void MontarFundo(Regiao r, int largura, int altura)
    {
        var pai = new GameObject("Fundo");

        // camada 0: cor chapada atrás de tudo, para nunca sobrar buraco
        var chapado = new GameObject("Chapado");
        chapado.transform.SetParent(pai.transform);
        var srChapado = chapado.AddComponent<SpriteRenderer>();
        srChapado.sprite = SpriteDeUmPixel();
        srChapado.color = CorProfunda(r);
        srChapado.sortingOrder = -200;
        srChapado.drawMode = SpriteDrawMode.Sliced;
        srChapado.size = new Vector2(largura * 3f, altura * 3f);
        chapado.transform.position = new Vector3(largura * 0.5f, altura * 0.5f, 30f);

        // camada 1: céu inteiro atrás. Ele só vai aparecer nos vãos entre as
        // folhas das camadas de mata - como um pedaço de luz entre as copas,
        // nunca como uma faixa aberta.
        var ceu = PrefabBuilder.SpriteSimples(r.fundo);
        if (ceu != null)
        {
            PreencherComCopias(pai.transform, "Ceu", ceu,
                x0: -largura * 0.4f, x1: largura * 1.4f,
                yBase: -2f, yTopo: altura * 1.5f,
                ordem: -160, z: 28f,
                cor: Color.Lerp(CorProfunda(r), Color.white, 0.45f),
                fator: 0.1f);
        }

        // Camadas de mata. A folha de árvores tem faixas verticais vazias
        // dentro dela, espaçadas de 7 unidades. Os deslocamentos abaixo são
        // frações ímpares desse período de propósito: qualquer múltiplo de 7
        // faria os vãos das camadas caírem uns sobre os outros e abriria
        // corredores de céu do chão ao topo.
        const float PeriodoDosVaos = 7f;
        var arvores = PrefabBuilder.SpriteSimples("Assets/Art/Environment/Forest/Trees/TreeLine.png");
        if (arvores != null)
        {
            // As alturas de partida também são escalonadas: a emenda entre
            // fileiras de uma camada cai no meio da folhagem da outra, em vez
            // de todas se alinharem numa faixa horizontal atravessando a tela.
            // As cores das camadas ficam próximas entre si de propósito. Com
            // contraste alto, o retângulo de cada cópia aparece como um bloco
            // escuro no fundo - e o jogador tenta pular em cima dele achando
            // que é plataforma.
            var baseDaMata = Color.Lerp(CorProfunda(r), r.tint, 0.30f);

            PreencherComCopias(pai.transform, "MataAoFundo", arvores,
                x0: -largura * 0.35f, x1: largura * 1.4f,
                yBase: -6f, yTopo: altura * 1.7f,
                ordem: -140, z: 26f,
                cor: baseDaMata,
                fator: 0.16f);

            PreencherComCopias(pai.transform, "MataMeia", arvores,
                x0: -largura * 0.3f + PeriodoDosVaos * 0.5f, x1: largura * 1.35f,
                yBase: -11f, yTopo: altura * 1.5f,
                ordem: -120, z: 24f,
                cor: Color.Lerp(baseDaMata, r.tint, 0.12f),
                fator: 0.22f);

            PreencherComCopias(pai.transform, "MataDistante", arvores,
                x0: -largura * 0.25f + PeriodoDosVaos * 0.25f, x1: largura * 1.3f,
                yBase: -3f, yTopo: altura * 1.3f,
                ordem: -90, z: 20f,
                cor: Color.Lerp(baseDaMata, r.tint, 0.24f),
                fator: 0.3f);

            PreencherComCopias(pai.transform, "MataProxima", arvores,
                x0: -largura * 0.15f + PeriodoDosVaos * 0.75f, x1: largura * 1.2f,
                yBase: -9f, yTopo: altura * 1f,
                ordem: -70, z: 15f,
                cor: Color.Lerp(baseDaMata, r.tint, 0.38f),
                fator: 0.5f);
        }
    }

    /// <summary>
    /// Cobre um retângulo com cópias do sprite, encostadas umas nas outras.
    /// Cada camada recebe o próprio fator de parallax.
    /// </summary>
    static void PreencherComCopias(Transform pai, string nome, Sprite sprite,
                                   float x0, float x1, float yBase, float yTopo,
                                   int ordem, float z, Color cor, float fator)
    {
        var camada = new GameObject(nome);
        camada.transform.SetParent(pai, false);

        var tamanho = sprite.bounds.size;
        if (tamanho.x <= 0.01f || tamanho.y <= 0.01f) return;

        // Passo exato, sem sobreposição. Sobrepor sprites com transparência
        // soma o alfa nas emendas e cria faixas visíveis - pior do que a
        // fresta que a sobreposição tentava evitar. Se sobrar uma fresta de
        // subpixel, quem aparece atrás é a camada seguinte de mata.
        float passoX = tamanho.x;
        float passoY = tamanho.y;

        for (float y = yBase; y < yTopo; y += passoY)
        {
            for (float x = x0; x < x1; x += passoX)
            {
                var go = new GameObject("Parte");
                go.transform.SetParent(camada.transform, false);
                go.transform.position = new Vector3(x + tamanho.x * 0.5f, y + tamanho.y * 0.5f, z);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = cor;
                sr.sortingOrder = ordem;
            }
        }

        var parallax = camada.AddComponent<ParallaxLayer>();
        parallax.fator = fator;
        parallax.travarVertical = true;
    }

    /// <summary>Cor do vazio absoluto da região - o que fica atrás de tudo.</summary>
    static Color CorProfunda(Regiao r)
    {
        if (r.arquivo.StartsWith("04_")) return new Color(0.05f, 0.07f, 0.10f);  // caverna
        if (r.arquivo.StartsWith("05_")) return new Color(0.09f, 0.05f, 0.12f);  // santuário
        return new Color(0.08f, 0.13f, 0.12f);                                    // mata
    }

    static Sprite pixelBranco;
    static Sprite SpriteDeUmPixel()
    {
        if (pixelBranco != null) return pixelBranco;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        pixelBranco = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return pixelBranco;
    }

    // ------------------------------------------------- sistemas, câmera, HUD
    static GameObject MontarSistemasEJogador(Regiao r, int largura, int altura)
    {
        // Os singletons vivem em todas as cenas: o Awake descarta as duplicatas.
        // Assim dá para abrir qualquer região direto no editor e jogar.
        var sistemas = new GameObject("_Sistemas");
        sistemas.AddComponent<GameManager>();
        sistemas.AddComponent<SaveSystem>();
        sistemas.AddComponent<MessageUI>();
        AdicionarTrilha(sistemas, r.arquivo);

        // fora do _Sistemas de propósito: aquele objeto atravessa as cenas
        var cursor = new GameObject("Cursor");
        cursor.AddComponent<ControleDoCursor>().mostrarCursor = false;

        var prefabJogador = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kaida.prefab");
        Vector2 inicio = AcharCaractere(r.mapa, 'P', largura, altura);
        if (inicio == Vector2.zero) inicio = AcharCaractere(r.mapa, 'p', largura, altura);

        var jogador = (GameObject)PrefabUtility.InstantiatePrefab(prefabJogador);
        jogador.name = "Kaida";
        // meia unidade de folga: nascer encostado no chão às vezes faz o
        // primeiro teste de colisão falhar e o jogador atravessa
        jogador.transform.position = inicio + Vector2.up * 0.5f;

        var hud = new GameObject("HUD");
        var health = hud.AddComponent<HealthUI>();
        health.player = jogador.GetComponent<PlayerController>();
        // frasco vermelho como pip: um quadrado colorido não diz nada
        health.fullIcon = RecorteDeSprites.Carregar(RecorteDeSprites.FrascoVida);

        // cada região tem sua pausa e sua tela de morte
        var pausa = new GameObject("MenuDePausa");
        pausa.AddComponent<PauseMenu>();

        var gameOver = new GameObject("TelaDeMorte");
        gameOver.AddComponent<GameOverUI>();

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;                 // mostra ~16 tiles de altura
        cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();

        var seguir = camGO.AddComponent<CameraFollow2D>();
        seguir.target = jogador.transform;
        seguir.offset = new Vector3(0f, 1.5f, -10f);
        seguir.smoothTime = 0.16f;
        // limites do mapa: a câmera calcula a margem sozinha conforme a
        // proporção da tela de quem está jogando
        seguir.useBounds = true;
        seguir.limiteMundoMin = new Vector2(0f, 0f);
        seguir.limiteMundoMax = new Vector2(largura, altura);
        camGO.transform.position = new Vector3(inicio.x, inicio.y + 1.5f, -10f);

        // a região do chefe é a última da progressão, não um número fixo:
        // inserir uma região no meio já quebrou isso uma vez
        if (string.IsNullOrEmpty(r.proxima))
        {
            var bossHud = new GameObject("BossHUD");
            bossHud.AddComponent<BossHealthUI>();

            var vitoria = new GameObject("TelaDeVitoria");
            vitoria.AddComponent<VictoryUI>();
        }

        return jogador;
    }

    // -------------------------------------------------------------- povoar
    static void PovoarMapa(Regiao r, int largura, int altura, GameObject jogador)
    {
        var conteudo = new GameObject("Conteudo");
        var pontosDeEco = new List<Transform>();

        if (r.pocoDeEscalada.width > 0f)
        {
            var poco = new GameObject("PocoDeEscalada");
            poco.transform.SetParent(conteudo.transform);
            poco.transform.position = r.pocoDeEscalada.center;
            poco.AddComponent<PocoDeEscalada>().area = r.pocoDeEscalada;
        }

        for (int linha = 0; linha < altura; linha++)
        {
            for (int coluna = 0; coluna < largura; coluna++)
            {
                char c = r.mapa[linha][coluna];
                Vector2 pos = new Vector2(coluna, altura - 1 - linha);

                switch (c)
                {
                    case 'P': CriarSpawn(conteudo, "inicio", pos); break;
                    case 'p': CriarSpawn(conteudo, "voltando", pos); break;

                    case 'C': Instanciar("Assets/Prefabs/Checkpoint.prefab", conteudo, pos); break;
                    case 'B': Instanciar("Assets/Prefabs/Inimigo_JavaliCasca.prefab", conteudo, pos); break;
                    case 'A': Instanciar("Assets/Prefabs/Inimigo_AbelhaEco.prefab", conteudo, pos); break;
                    case 'S': Instanciar("Assets/Prefabs/Inimigo_CaracolRastejante.prefab", conteudo, pos); break;

                    case 'H': CriarHabilidade(r, conteudo, pos); break;
                    case 'F': CriarFragmento(r, conteudo, pos); break;
                    case 'N': CriarNodulo(r, conteudo, pos); break;
                    case 'X': CriarPerigo(conteudo, pos); break;

                    case '>': CriarTransicao(conteudo, pos, r.proxima, "voltando"); break;
                    case '<': CriarTransicao(conteudo, pos, r.anterior, "chegando"); break;

                    case 'G': CriarChefe(conteudo, pos, jogador, pontosDeEco, largura, altura); break;
                }
            }
        }

        // ponto de chegada de quem volta da região seguinte
        if (AcharCaractere(r.mapa, 'p', largura, altura) == Vector2.zero)
        {
            var saida = AcharCaractere(r.mapa, '>', largura, altura);
            if (saida != Vector2.zero) CriarSpawn(conteudo, "chegando", saida + Vector2.left * 2f);
        }
    }

    static void CriarSpawn(GameObject pai, string id, Vector2 pos)
    {
        var go = new GameObject($"Spawn_{id}");
        go.transform.SetParent(pai.transform);
        go.transform.position = pos;
        go.AddComponent<SpawnPoint>().id = id;
    }

    static GameObject Instanciar(string caminhoPrefab, GameObject pai, Vector2 pos)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminhoPrefab);
        if (prefab == null)
        {
            Debug.LogWarning("[Kaida] Prefab não encontrado: " + caminhoPrefab);
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(pai.transform);
        go.transform.position = pos;
        return go;
    }

    static void CriarHabilidade(Regiao r, GameObject pai, Vector2 pos)
    {
        var go = Instanciar("Assets/Prefabs/PickupHabilidade.prefab", pai, pos);
        if (go == null) return;
        var p = go.GetComponent<PickupAbility>();

        if (r.arquivo.StartsWith("02_"))
        {
            p.abilityId = "double_jump";
            p.mensagem = "Pulo Duplo\nO ar segura você por um instante a mais.";
        }
        else
        {
            p.abilityId = "wall_climb";
            p.mensagem = "Escalada de Parede\nA pedra lembra de quem se apoiou nela.";
        }
    }

    static int contadorFragmento = 0;
    static void CriarFragmento(Regiao r, GameObject pai, Vector2 pos)
    {
        var go = Instanciar("Assets/Prefabs/FragmentoDeLumen.prefab", pai, pos);
        if (go == null) return;

        var f = go.GetComponent<LoreFragment>();
        f.fragmentId = $"frag_{r.arquivo}_{contadorFragmento++}";
        f.texto = TextoDeLore(r.arquivo);
    }

    /// <summary>Os fragmentos contam a história aos poucos, na ordem das regiões.</summary>
    static string TextoDeLore(string regiao)
    {
        if (regiao.StartsWith("01_"))
            return "\"Acordei na orla sem saber o próprio nome.\nA espada nas costas parecia me conhecer\nmelhor do que eu.\"";
        if (regiao.StartsWith("02_"))
            return "\"A floresta guardava a memória do povo.\nQuando o Esquecimento veio, ela escureceu\nprimeiro - como quem fecha os olhos.\"";
        if (regiao.StartsWith("03_"))
            return "\"Tiravam lúmen daqui. Diziam que a pedra\nsegurava lembranças. Ninguém perguntou\nde quem eram.\"";
        return "\"Parar também é uma forma de esquecer.\"";
    }

    static int contadorNodulo = 0;
    static void CriarNodulo(Regiao r, GameObject pai, Vector2 pos)
    {
        var go = Instanciar("Assets/Prefabs/NoduloDeVida.prefab", pai, pos);
        if (go == null) return;
        go.GetComponent<HealthNode>().nodeId = $"node_{r.arquivo}_{contadorNodulo++}";
    }

    static void CriarPerigo(GameObject pai, Vector2 pos)
    {
        var go = new GameObject("Perigo");
        go.transform.SetParent(pai.transform);
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        var h = go.AddComponent<Hazard>();
        h.damage = 1;
        h.returnToCheckpoint = true;
        h.repeatInterval = 0.9f;
    }

    static void CriarTransicao(GameObject pai, Vector2 pos, string destino, string spawnDestino)
    {
        if (string.IsNullOrEmpty(destino)) return;

        var go = new GameObject($"Passagem_para_{destino}");
        go.transform.SetParent(pai.transform);
        go.transform.position = pos + Vector2.up * 1.5f;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 6f);

        var t = go.AddComponent<RoomTransition>();
        t.targetScene = destino;
        t.targetSpawnId = spawnDestino;
    }

    static void CriarChefe(GameObject pai, Vector2 pos, GameObject jogador,
                           List<Transform> pontos, int largura, int altura)
    {
        var go = Instanciar("Assets/Prefabs/GuardiaoDoLumen.prefab", pai, pos);
        if (go == null) return;

        var boss = go.GetComponent<GuardianBoss>();
        if (jogador != null) boss.player = jogador.transform;

        // quatro cantos da arena para os ecos aparecerem longe do jogador
        var lista = new List<Transform>();
        Vector2[] cantos =
        {
            new Vector2(pos.x - 12f, pos.y - 5f),
            new Vector2(pos.x + 12f, pos.y - 5f),
            new Vector2(pos.x - 7f,  pos.y + 2f),
            new Vector2(pos.x + 7f,  pos.y + 2f),
        };
        for (int i = 0; i < cantos.Length; i++)
        {
            var p = new GameObject($"PontoDeEco_{i}");
            p.transform.SetParent(pai.transform);
            p.transform.position = cantos[i];
            lista.Add(p.transform);
        }
        boss.pontosDeInvocacao = lista.ToArray();
    }

    static Vector2 AcharCaractere(string[] mapa, char alvo, int largura, int altura)
    {
        for (int linha = 0; linha < altura; linha++)
            for (int coluna = 0; coluna < largura; coluna++)
                if (mapa[linha][coluna] == alvo)
                    return new Vector2(coluna, altura - 1 - linha);
        return Vector2.zero;
    }
}
