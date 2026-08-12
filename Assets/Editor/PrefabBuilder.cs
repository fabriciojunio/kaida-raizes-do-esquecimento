using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Monta os prefabs jogáveis: Kaida, os três inimigos, o Guardião, o feixe e
/// os objetos de mundo (checkpoint, habilidade, fragmento, nódulo).
///
/// Escala: 16 pixels por unidade, então 1 tile = 1 unidade. Nessa medida
/// Kaida tem 3 unidades de altura e o javali 2,5 de largura - as proporções
/// vêm do próprio pacote de arte, não são arbitrárias.
/// </summary>
public static class PrefabBuilder
{
    const string PastaPrefabs = "Assets/Prefabs";

    // índices do TagManager.asset
    public const int LayerGround = 6;
    public const int LayerPlayer = 7;
    public const int LayerEnemy  = 8;

    public static LayerMask MaskGround => 1 << LayerGround;
    public static LayerMask MaskPlayer => 1 << LayerPlayer;
    public static LayerMask MaskEnemy  => 1 << LayerEnemy;

    [MenuItem("Kaida/3. Gerar prefabs")]
    public static void GerarTudo()
    {
        Pasta(PastaPrefabs);

        var stats = CriarPlayerStats();
        CriarKaida(stats);
        CriarFeixe();
        CriarJavali();
        CriarAbelha();
        CriarCaracol();
        CriarGuardiao();
        CriarObjetosDeMundo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Kaida] Prefabs gerados.");
    }

    // ------------------------------------------------------------------ stats
    static PlayerStats CriarPlayerStats()
    {
        Pasta("Assets/Settings");
        const string caminho = "Assets/Settings/PlayerStats.asset";

        var stats = AssetDatabase.LoadAssetAtPath<PlayerStats>(caminho);
        if (stats == null)
        {
            stats = ScriptableObject.CreateInstance<PlayerStats>();
            AssetDatabase.CreateAsset(stats, caminho);
        }

        // valores pensados para a escala do jogo (1 unidade = 1 tile de 16px)
        stats.runSpeed = 7.5f;
        stats.groundAccel = 90f;
        stats.groundDecel = 100f;
        stats.airAccel = 60f;
        stats.airDecel = 45f;

        stats.jumpHeight = 4.0f;          // pouco mais que a própria altura
        stats.jumpTimeToPeak = 0.38f;
        stats.jumpTimeToDescent = 0.32f;
        stats.jumpCutMultiplier = 0.45f;
        stats.maxFallSpeed = 24f;

        stats.coyoteTime = 0.11f;
        stats.jumpBufferTime = 0.13f;

        stats.airJumps = 1;
        stats.airJumpPower = 0.92f;
        stats.wallSlideSpeed = 3.5f;
        stats.wallJumpForceX = 9.5f;
        stats.wallJumpPower = 1f;
        stats.wallJumpLockTime = 0.16f;

        stats.dashSpeed = 19f;
        stats.dashTime = 0.17f;
        stats.dashCooldown = 0.4f;
        stats.airDashes = 1;

        stats.maxHealth = 5;
        stats.attackDamage = 1;
        stats.invulnTime = 1.0f;
        stats.knockbackForce = 10f;

        EditorUtility.SetDirty(stats);
        return stats;
    }

    // ------------------------------------------------------------------ Kaida
    static void CriarKaida(PlayerStats stats)
    {
        var go = new GameObject("Kaida");
        go.layer = LayerPlayer;
        go.tag = "Player";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimeiroSprite("Assets/Art/Player/Kaida/Kaida-Idle.png");
        sr.sortingOrder = 10;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;             // o PlayerController aplica a gravidade
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Estreito e com a base logo acima do pivô. Um colisor largo engancha
        // na quina das plataformas: o mesmo salto funcionava de um lado e
        // falhava do outro, dependendo de qual borda a personagem raspava.
        var col = go.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        col.size = new Vector2(0.85f, 2.7f);
        col.offset = new Vector2(0f, 1.36f);    // o pivô do sprite está nos pés
        col.sharedMaterial = MaterialSemAtrito();

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Kaida/Kaida.controller");
        anim.applyRootMotion = false;

        var groundCheck = Filho(go, "GroundCheck", new Vector3(0f, 0.06f, 0f));
        var attackPoint = Filho(go, "AttackPoint", new Vector3(1.5f, 1.4f, 0f));
        var wallCheck   = Filho(go, "WallCheck",   new Vector3(0.5f, 1.6f, 0f));

        var pc = go.AddComponent<PlayerController>();
        pc.stats = stats;
        pc.groundCheck = groundCheck.transform;
        // raio um pouco maior que a metade do colisor: a personagem reconhece
        // que está no chão mesmo pisando na beirada de uma plataforma
        pc.groundCheckRadius = 0.42f;
        pc.groundLayer = MaskGround;
        pc.attackPoint = attackPoint.transform;
        // O alcance acompanha a espada, que é longa: com 0,95 só acertava
        // quem estivesse praticamente colado na personagem.
        pc.attackRadius = 1.35f;
        pc.enemyLayer = MaskEnemy;
        pc.wallCheck = wallCheck.transform;
        pc.wallCheckDistance = 0.35f;
        pc.animator = anim;
        pc.spriteRenderer = sr;

        Salvar(go, $"{PastaPrefabs}/Kaida.prefab");
    }

    // ------------------------------------------------------------------ feixe
    static void CriarFeixe()
    {
        var go = new GameObject("LumenBeam");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = RecorteDeSprites.Carregar(RecorteDeSprites.FrascoLumen);
        sr.color = new Color(1f, 0.9f, 0.55f);
        sr.sortingOrder = 12;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;
        col.offset = new Vector2(0f, 0.5f);

        var beam = go.AddComponent<LumenBeam>();
        beam.dano = 1;
        beam.tempoDeVida = 5f;
        beam.paredes = MaskGround;

        Salvar(go, $"{PastaPrefabs}/LumenBeam.prefab");
    }

    // ---------------------------------------------------------------- inimigos
    static GameObject BaseInimigo(string nome, string spriteFolha, string controller,
                                  Vector2 tamanhoCollider, Vector2 offsetCollider)
    {
        var go = new GameObject(nome);
        go.layer = LayerEnemy;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimeiroSprite(spriteFolha);
        sr.sortingOrder = 8;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = tamanhoCollider;
        col.offset = offsetCollider;
        col.sharedMaterial = MaterialSemAtrito();

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controller);
        anim.applyRootMotion = false;

        return go;
    }

    static void ConfigurarInimigo(EnemyController ec, GameObject go, float alturaGroundCheck)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        ec.animator = go.GetComponent<Animator>();
        ec.spriteRenderer = sr;
        ec.groundLayer = MaskGround;
        ec.playerLayer = MaskPlayer;
        ec.groundCheck = Filho(go, "GroundCheck", new Vector3(0.7f, alturaGroundCheck, 0f)).transform;
        ec.wallCheck   = Filho(go, "WallCheck",   new Vector3(0.7f, alturaGroundCheck + 0.4f, 0f)).transform;
    }

    static void CriarJavali()
    {
        var go = BaseInimigo("JavaliCasca", "Assets/Art/Enemies/Boar/Boar-Idle.png",
                             "Assets/Animations/Boar/Boar.controller",
                             new Vector2(2.2f, 1.5f), new Vector2(0f, 0.8f));

        var e = go.AddComponent<BoarEnemy>();
        e.maxHealth = 3;
        e.contactDamage = 1;
        e.moveSpeed = 2.2f;
        e.detectRange = 5.5f;
        e.attackRange = 1.2f;
        e.attackCooldown = 1.1f;
        e.telegraphTime = 0.45f;
        e.chargeSpeed = 8.5f;
        e.chargeDuration = 0.9f;
        e.recoverTime = 0.8f;
        ConfigurarInimigo(e, go, 0.15f);

        Salvar(go, $"{PastaPrefabs}/Inimigo_JavaliCasca.prefab");
    }

    static void CriarAbelha()
    {
        var go = BaseInimigo("AbelhaEco", "Assets/Art/Enemies/Bee/Bee-Fly.png",
                             "Assets/Animations/Bee/Bee.controller",
                             new Vector2(1.1f, 1.1f), new Vector2(0f, 1.4f));
        go.GetComponent<Rigidbody2D>().gravityScale = 0f;   // voa

        var e = go.AddComponent<BeeEnemy>();
        e.maxHealth = 2;
        e.contactDamage = 1;
        e.moveSpeed = 2f;
        e.detectRange = 6f;
        e.attackRange = 0.9f;
        e.attackCooldown = 1.2f;
        e.amplitude = 0.7f;
        e.frequency = 2.2f;
        e.horizontalRange = 3f;
        e.diveSpeed = 9f;
        e.diveCooldown = 2.4f;
        e.requireLineOfSight = true;
        ConfigurarInimigo(e, go, 0.6f);

        Salvar(go, $"{PastaPrefabs}/Inimigo_AbelhaEco.prefab");
    }

    static void CriarCaracol()
    {
        var go = BaseInimigo("CaracolRastejante", "Assets/Art/Enemies/Snail/Snail-Walk.png",
                             "Assets/Animations/Snail/Snail.controller",
                             new Vector2(1.5f, 1.2f), new Vector2(0f, 0.65f));

        var e = go.AddComponent<SnailEnemy>();
        e.maxHealth = 4;
        e.contactDamage = 1;
        e.moveSpeed = 1.1f;
        e.detectRange = 3.5f;
        e.attackRange = 1f;
        e.attackCooldown = 1.4f;
        e.hideDuration = 1.6f;
        e.hitsBeforeHiding = 1;
        ConfigurarInimigo(e, go, 0.12f);

        Salvar(go, $"{PastaPrefabs}/Inimigo_CaracolRastejante.prefab");
    }

    // ----------------------------------------------------------------- chefe
    static void CriarGuardiao()
    {
        var go = new GameObject("GuardiaoDoLumen");
        go.layer = LayerEnemy;
        go.transform.localScale = Vector3.one * 2.1f;   // presença de chefe

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimeiroSprite("Assets/Art/Enemies/Bee/Bee-Fly.png");
        sr.color = new Color(0.72f, 0.8f, 1f);          // corrompido pelo lúmen
        sr.sortingOrder = 9;

        // Cinemático: ele voa e é movido por código, não pela física.
        //
        // Como corpo dinâmico, ele pousava em cima das plataformas da arena.
        // A física zerava a velocidade vertical e o Guardião passava a luta
        // inteira empoleirado, alto demais para o golpe corpo a corpo - o
        // confronto simplesmente não tinha saída.
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Colisor generoso e baixo, como gatilho: serve para receber o golpe,
        // não para esbarrar no cenário.
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 1.15f;
        col.offset = new Vector2(0f, 0.9f);
        col.isTrigger = true;

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Guardian/Guardian.controller");

        var origem = Filho(go, "BeamOrigin", new Vector3(0f, 1.2f, 0f));

        var boss = go.AddComponent<GuardianBoss>();
        boss.healthFase1 = 12;
        boss.healthFase2 = 14;
        boss.healthFase3 = 16;
        boss.beamPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaPrefabs}/LumenBeam.prefab");
        boss.beamOrigin = origem.transform;
        // Ritmo com folga para ler e revidar. Os valores anteriores não davam
        // espaço entre um ataque e o seguinte.
        boss.beamInterval = 2.4f;
        boss.beamSpeed = 6f;
        boss.beamsPorSalva = 3;
        boss.ecosPorOnda = 2;
        boss.intervaloEntreOndas = 8f;
        boss.velocidadeInvestida = 6f;
        boss.intervaloInvestida = 2.4f;
        boss.danoContato = 1;
        boss.animator = anim;
        boss.spriteRenderer = sr;
        boss.playerLayer = MaskPlayer;
        boss.ecoPrefabs = new[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaPrefabs}/Inimigo_JavaliCasca.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaPrefabs}/Inimigo_AbelhaEco.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaPrefabs}/Inimigo_CaracolRastejante.prefab"),
        };

        Salvar(go, $"{PastaPrefabs}/GuardiaoDoLumen.prefab");
    }

    // -------------------------------------------------------- objetos de mundo
    static void CriarObjetosDeMundo()
    {
        // Cada objeto usa uma peça recortada do tileset. Usar a folha inteira
        // colocaria uma imagem de dezenas de unidades no meio do cenário.

        // marco de descanso: uma tocha acesa
        var cp = new GameObject("Checkpoint");
        var cpSr = cp.AddComponent<SpriteRenderer>();
        cpSr.sprite = RecorteDeSprites.Carregar(RecorteDeSprites.Tocha);
        cpSr.sortingOrder = 4;
        var cpCol = cp.AddComponent<BoxCollider2D>();
        cpCol.isTrigger = true;
        cpCol.size = new Vector2(2f, 3f);
        cpCol.offset = new Vector2(0f, 1.5f);
        cp.AddComponent<Checkpoint>();
        Salvar(cp, $"{PastaPrefabs}/Checkpoint.prefab");

        // habilidade largada no mundo: uma chave
        var hab = new GameObject("PickupHabilidade");
        var habSr = hab.AddComponent<SpriteRenderer>();
        habSr.sprite = RecorteDeSprites.Carregar(RecorteDeSprites.Chave);
        habSr.color = new Color(1f, 0.95f, 0.7f);
        habSr.sortingOrder = 6;
        var habCol = hab.AddComponent<CircleCollider2D>();
        habCol.isTrigger = true;
        habCol.radius = 1.2f;
        habCol.offset = new Vector2(0f, 0.5f);
        hab.AddComponent<PickupAbility>();
        Salvar(hab, $"{PastaPrefabs}/PickupHabilidade.prefab");

        // fragmento de lúmen: frasco de luz
        var frag = new GameObject("FragmentoDeLumen");
        var fragSr = frag.AddComponent<SpriteRenderer>();
        fragSr.sprite = RecorteDeSprites.Carregar(RecorteDeSprites.Medalhao);
        fragSr.sortingOrder = 6;
        var fragCol = frag.AddComponent<CircleCollider2D>();
        fragCol.isTrigger = true;
        fragCol.radius = 1.1f;
        fragCol.offset = new Vector2(0f, 1f);
        frag.AddComponent<LoreFragment>();
        Salvar(frag, $"{PastaPrefabs}/FragmentoDeLumen.prefab");

        // nódulo de vida: frasco vermelho
        var no = new GameObject("NoduloDeVida");
        var noSr = no.AddComponent<SpriteRenderer>();
        noSr.sprite = RecorteDeSprites.Carregar(RecorteDeSprites.FrascoVida);
        noSr.sortingOrder = 6;
        var noCol = no.AddComponent<CircleCollider2D>();
        noCol.isTrigger = true;
        noCol.radius = 1.1f;
        noCol.offset = new Vector2(0f, 1f);
        no.AddComponent<HealthNode>();
        Salvar(no, $"{PastaPrefabs}/NoduloDeVida.prefab");
    }

    // ------------------------------------------------------------- utilidades
    static PhysicsMaterial2D materialSemAtrito;
    static PhysicsMaterial2D MaterialSemAtrito()
    {
        if (materialSemAtrito != null) return materialSemAtrito;
        const string caminho = "Assets/Settings/SemAtrito.physicsMaterial2D";
        materialSemAtrito = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(caminho);
        if (materialSemAtrito == null)
        {
            Pasta("Assets/Settings");
            // sem isso o personagem gruda nas paredes ao encostar de lado
            materialSemAtrito = new PhysicsMaterial2D("SemAtrito") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(materialSemAtrito, caminho);
        }
        return materialSemAtrito;
    }

    static GameObject Filho(GameObject pai, string nome, Vector3 posLocal)
    {
        var f = new GameObject(nome);
        f.transform.SetParent(pai.transform, false);
        f.transform.localPosition = posLocal;
        return f;
    }

    public static Sprite PrimeiroSprite(string caminhoFolha)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(caminhoFolha).OfType<Sprite>().ToArray();
        if (sprites.Length == 0) return null;
        return sprites.OrderBy(s => s.name).First();
    }

    public static Sprite SpriteSimples(string caminho)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s != null) return s;
        return AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().FirstOrDefault();
    }

    static void Salvar(GameObject go, string caminho)
    {
        PrefabUtility.SaveAsPrefabAsset(go, caminho);
        Object.DestroyImmediate(go);
    }

    public static void Pasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder(caminho)) return;
        string pai = System.IO.Path.GetDirectoryName(caminho).Replace('\\', '/');
        string nome = System.IO.Path.GetFileName(caminho);
        if (!AssetDatabase.IsValidFolder(pai)) Pasta(pai);
        AssetDatabase.CreateFolder(pai, nome);
    }
}
