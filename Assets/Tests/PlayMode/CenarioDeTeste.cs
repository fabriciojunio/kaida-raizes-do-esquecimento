using UnityEngine;

/// <summary>
/// Monta um cenário mínimo de verdade para os testes de PlayMode: chão com
/// collider, Kaida com Rigidbody e a física rodando. Nada é simulado - o que
/// os testes medem é o mesmo comportamento que aparece no jogo.
/// </summary>
public static class CenarioDeTeste
{
    public const int LayerGround = 6;
    public const int LayerPlayer = 7;
    public const int LayerEnemy = 8;

    public static LayerMask MaskGround => 1 << LayerGround;
    public static LayerMask MaskPlayer => 1 << LayerPlayer;
    public static LayerMask MaskEnemy => 1 << LayerEnemy;

    public static PlayerStats StatsPadrao()
    {
        var s = ScriptableObject.CreateInstance<PlayerStats>();
        s.runSpeed = 7.5f;
        s.groundAccel = 90f; s.groundDecel = 100f;
        s.airAccel = 60f; s.airDecel = 45f;
        s.jumpHeight = 4f; s.jumpTimeToPeak = 0.38f; s.jumpTimeToDescent = 0.32f;
        s.jumpCutMultiplier = 0.45f; s.maxFallSpeed = 24f;
        s.coyoteTime = 0.11f; s.jumpBufferTime = 0.13f;
        s.airJumps = 1; s.airJumpPower = 0.92f;
        s.dashSpeed = 19f; s.dashTime = 0.17f; s.dashCooldown = 0.4f; s.airDashes = 1;
        s.maxHealth = 5; s.attackDamage = 1; s.invulnTime = 1f; s.knockbackForce = 10f;
        return s;
    }

    /// <summary>Plataforma sólida na layer Ground.</summary>
    public static GameObject CriarChao(Vector2 centro, Vector2 tamanho)
    {
        var go = new GameObject("Chao") { layer = LayerGround };
        go.transform.position = centro;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = tamanho;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        return go;
    }

    public static GameObject CriarParede(Vector2 centro, Vector2 tamanho) => CriarChao(centro, tamanho);

    /// <summary>
    /// Kaida montada como no prefab, mas sem depender dele.
    ///
    /// Nasce desativada: o Awake clona os stats, aplica a dificuldade e define
    /// a vida inicial. Atribuir `stats` depois de ela acordar faria o teste
    /// medir uma configuração diferente da que está valendo.
    /// </summary>
    public static PlayerController CriarKaida(Vector2 posicao, PlayerStats stats = null)
    {
        var go = new GameObject("Kaida") { layer = LayerPlayer };
        go.SetActive(false);
        go.transform.position = posicao;

        var sr = go.AddComponent<SpriteRenderer>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        col.size = new Vector2(1.15f, 2.85f);
        col.offset = new Vector2(0f, 1.44f);
        col.sharedMaterial = new PhysicsMaterial2D("semAtrito") { friction = 0f, bounciness = 0f };

        var groundCheck = Filho(go, "GroundCheck", new Vector3(0f, 0.06f, 0f));
        var attackPoint = Filho(go, "AttackPoint", new Vector3(1.1f, 1.5f, 0f));

        var pc = go.AddComponent<PlayerController>();
        pc.stats = stats != null ? stats : StatsPadrao();
        pc.groundCheck = groundCheck;
        pc.groundCheckRadius = 0.22f;
        pc.groundLayer = MaskGround;
        pc.attackPoint = attackPoint;
        pc.attackRadius = 0.95f;
        pc.enemyLayer = MaskEnemy;
        pc.spriteRenderer = sr;

        go.SetActive(true);   // só agora o Awake roda, com tudo no lugar
        return pc;
    }

    /// <summary>
    /// Inimigo genérico, sem animator (os testes olham comportamento, não arte).
    ///
    /// Nasce desativado de propósito: o Awake copia maxHealth para a vida
    /// atual, então qualquer ajuste feito depois de o objeto acordar não teria
    /// efeito nenhum. Ajuste os valores dentro de `configurar`.
    /// </summary>
    public static T CriarInimigo<T>(Vector2 posicao, bool voador = false,
                                    System.Action<T> configurar = null) where T : EnemyController
    {
        var go = new GameObject(typeof(T).Name) { layer = LayerEnemy };
        go.SetActive(false);          // segura o Awake até a configuração terminar
        go.transform.position = posicao;

        go.AddComponent<SpriteRenderer>();
        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = voador ? 0f : 3f;

        var col = go.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = new Vector2(1.5f, 1.2f);
        col.offset = new Vector2(0f, 0.65f);

        var e = go.AddComponent<T>();
        e.groundLayer = MaskGround;
        e.playerLayer = MaskPlayer;
        e.spriteRenderer = go.GetComponent<SpriteRenderer>();
        e.groundCheck = Filho(go, "GroundCheck", new Vector3(0.7f, 0.15f, 0f));
        e.wallCheck = Filho(go, "WallCheck", new Vector3(0.7f, 0.55f, 0f));

        configurar?.Invoke(e);
        go.SetActive(true);           // agora o Awake roda com os valores certos
        return e;
    }

    /// <summary>
    /// Os singletons que o gameplay consulta.
    ///
    /// Fixa a dificuldade em Normal: ela fica em PlayerPrefs, então sem isso
    /// os testes mediriam a última dificuldade que a pessoa escolheu jogando,
    /// e passariam ou falhariam conforme a máquina.
    /// </summary>
    public static GameObject CriarSistemas()
    {
        GameSettings.Atual = Dificuldade.Normal;

        var go = new GameObject("_Sistemas");
        go.AddComponent<GameManager>();
        var save = go.AddComponent<SaveSystem>();
        save.Data = new SaveData();
        return go;
    }

    static Transform Filho(GameObject pai, string nome, Vector3 local)
    {
        var f = new GameObject(nome);
        f.transform.SetParent(pai.transform, false);
        f.transform.localPosition = local;
        return f.transform;
    }

    /// <summary>
    /// Limpa tudo entre um teste e outro, inclusive os singletons e o que
    /// tiver sobrado de uma cena real carregada por outro teste.
    ///
    /// Sem essa varredura, os testes que abrem as regiões de verdade deixavam
    /// inimigos e o Guardião vivos em cena; o teste seguinte montava o próprio
    /// cenário por cima e media o objeto errado.
    /// </summary>
    public static void Limpar()
    {
        foreach (var go in Object.FindObjectsOfType<GameObject>(true))
        {
            if (go == null) continue;
            if (go.transform.parent != null) continue;   // filhos vão junto do pai
            Object.DestroyImmediate(go);
        }
    }

    /// <summary>
    /// Começa o teste num ambiente vazio, sem restos de cena anterior.
    /// Chamado no início de cada SetUp.
    /// </summary>
    public static void PrepararAmbiente()
    {
        Limpar();
        Time.timeScale = 1f;
        Physics2D.IgnoreLayerCollision(LayerPlayer, LayerEnemy, true);
        GameSettings.Atual = Dificuldade.Normal;
    }
}
