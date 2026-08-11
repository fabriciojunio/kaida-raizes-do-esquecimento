using UnityEngine;

/// <summary>
/// O Guardião do Lúmen (Santuário Esquecido). Três fases, cada uma cobrando
/// uma habilidade diferente que Kaida juntou pelo caminho:
///   1. feixes de memória à distância  -> dash
///   2. ecos dos inimigos já vencidos  -> combate aprendido
///   3. corpo a corpo encurralando     -> pulo duplo e parede
/// Reaproveita a mesma State/StateMachine do jogador.
/// </summary>
public class GuardianBoss : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public int healthFase1 = 12;
    public int healthFase2 = 14;
    public int healthFase3 = 16;

    [Header("Fase 1 - feixes")]
    public GameObject beamPrefab;
    public Transform beamOrigin;
    public float beamInterval = 1.5f;
    public float beamSpeed = 7f;
    public int beamsPorSalva = 3;

    [Header("Fase 2 - ecos")]
    public GameObject[] ecoPrefabs;
    public Transform[] pontosDeInvocacao;
    public int ecosPorOnda = 3;
    public float intervaloEntreOndas = 6f;

    [Header("Fase 3 - corpo a corpo")]
    public float velocidadeInvestida = 7.5f;
    public float intervaloInvestida = 1.6f;
    public int danoContato = 1;

    [Header("Referências")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform player;
    public LayerMask playerLayer;

    public StateMachine Machine { get; private set; }
    public int FaseAtual { get; private set; } = 1;
    public int Health { get; private set; }
    public bool Derrotado { get; private set; }

    public System.Action<int, int> HealthChanged;   // (atual, max da fase)
    public System.Action<int> FaseMudou;
    public System.Action Morreu;

    Rigidbody2D rb;

    public Rigidbody2D Body => rb;
    public int MaxHealthFaseAtual =>
        FaseAtual == 1 ? healthFase1 : FaseAtual == 2 ? healthFase2 : healthFase3;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0f; rb.freezeRotation = true; }
        Health = healthFase1;

        Machine = new StateMachine();
        Machine.Add("intro", new BossIntroState(this));
        Machine.Add("fase1", new BossPhase1State(this));
        Machine.Add("fase2", new BossPhase2State(this));
        Machine.Add("fase3", new BossPhase3State(this));
        Machine.Add("transicao", new BossTransitionState(this));
        Machine.Add("morto", new BossDeadState(this));
    }

    void Start()
    {
        if (player == null)
        {
            var p = FindObjectOfType<PlayerController>();
            if (p != null) player = p.transform;
        }
        Machine.SetInitial("intro");
        HealthChanged?.Invoke(Health, MaxHealthFaseAtual);
    }

    void Update()   { if (!Derrotado) Machine.LogicUpdate(); }
    void FixedUpdate() { if (!Derrotado) Machine.PhysicsUpdate(); }

    public void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (Derrotado) return;
        // durante a transição entre fases ele fica intocável
        if (Machine.CurrentName == "transicao" || Machine.CurrentName == "intro") return;

        Health = Mathf.Max(0, Health - amount);
        HealthChanged?.Invoke(Health, MaxHealthFaseAtual);
        StartCoroutine(FlashRoutine());

        if (Health <= 0)
        {
            if (FaseAtual < 3) Machine.ChangeState("transicao");
            else Machine.ChangeState("morto");
        }
    }

    /// <summary>Chamado pelo estado de transição ao terminar a virada de fase.</summary>
    public void AvancarFase()
    {
        FaseAtual++;
        Health = MaxHealthFaseAtual;
        HealthChanged?.Invoke(Health, MaxHealthFaseAtual);
        FaseMudou?.Invoke(FaseAtual);
        Machine.ChangeState("fase" + FaseAtual);
    }

    public void MarcarDerrotado()
    {
        Derrotado = true;
        if (rb != null) rb.velocity = Vector2.zero;
        Morreu?.Invoke();
    }

    public void PlayAnim(string nome)
    {
        if (animator != null) animator.Play(nome);
    }

    /// <summary>Dispara um feixe de memória na direção do jogador.</summary>
    public void DispararFeixe(Vector2 direcao)
    {
        if (beamPrefab == null) return;
        Vector3 origem = beamOrigin != null ? beamOrigin.position : transform.position;
        var go = Instantiate(beamPrefab, origem, Quaternion.identity);
        var beam = go.GetComponent<LumenBeam>();
        if (beam != null) beam.Lancar(direcao.normalized, beamSpeed);
        else if (go.GetComponent<Rigidbody2D>() is Rigidbody2D brb)
            brb.velocity = direcao.normalized * beamSpeed;
    }

    /// <summary>Invoca ecos fracos dos inimigos que o jogador já enfrentou.</summary>
    public int InvocarEcos(int quantidade)
    {
        if (ecoPrefabs == null || ecoPrefabs.Length == 0) return 0;
        if (pontosDeInvocacao == null || pontosDeInvocacao.Length == 0) return 0;

        int criados = 0;
        for (int i = 0; i < quantidade; i++)
        {
            var prefab = ecoPrefabs[Random.Range(0, ecoPrefabs.Length)];
            var ponto = pontosDeInvocacao[i % pontosDeInvocacao.Length];
            if (prefab == null || ponto == null) continue;

            var eco = Instantiate(prefab, ponto.position, Quaternion.identity);
            // "eco": versão enfraquecida do inimigo original
            var ec = eco.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.maxHealth = Mathf.Max(1, ec.maxHealth / 2);
                ec.moveSpeed *= 1.15f;
            }
            var sr = eco.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.75f, 0.85f, 1f, 0.8f);
            criados++;
        }
        return criados;
    }

    public bool AindaTemEcos()
    {
        foreach (var e in FindObjectsOfType<EnemyController>())
            if (e != null && !e.Dying) return true;
        return false;
    }

    public void AtingirJogadorSeEncostou(float raio)
    {
        var hit = Physics2D.OverlapCircle(transform.position, raio, playerLayer);
        if (hit == null) return;
        var pc = hit.GetComponent<PlayerController>();
        if (pc != null) pc.TakeDamage(danoContato, transform.position);
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;
        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(1f, 0.8f, 0.8f);
        yield return new WaitForSeconds(0.08f);
        if (spriteRenderer != null) spriteRenderer.color = c;
    }
}
