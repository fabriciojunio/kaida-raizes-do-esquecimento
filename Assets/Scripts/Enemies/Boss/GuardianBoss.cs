using UnityEngine;

/// <summary>
/// O Guardião do Lúmen, no Santuário Esquecido. Um confronto só, direto: ele
/// tem uma barra de vida e cai quando ela zera.
///
/// Já foi dividido em três fases, com invocação de inimigos entre elas. Na
/// prática o combate virava matar horda enquanto o chefe ficava de fora da
/// própria luta - e o jogador chegava ao fim sem sentir que tinha enfrentado
/// alguém. Agora é ele e a Kaida.
///
/// Reaproveita a mesma State/StateMachine do jogador.
/// </summary>
public class GuardianBoss : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public int maxHealth = 20;

    [Header("Feixes de lúmen")]
    public GameObject beamPrefab;
    public Transform beamOrigin;
    public float beamInterval = 2.4f;
    public float beamSpeed = 6f;
    public int beamsPorSalva = 3;

    [Header("Investida")]
    public float velocidadeInvestida = 6f;
    public float intervaloInvestida = 3.2f;
    public int danoContato = 1;

    [Header("Referências")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform player;
    public LayerMask playerLayer;

    public StateMachine Machine { get; private set; }
    public int Health { get; private set; }
    public bool Derrotado { get; private set; }

    public System.Action<int, int> HealthChanged;
    public System.Action Morreu;

    Rigidbody2D rb;

    public Rigidbody2D Body => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0f; rb.freezeRotation = true; }
        Health = maxHealth;

        Machine = new StateMachine();
        Machine.Add("intro", new BossIntroState(this));
        Machine.Add("combate", new BossCombateState(this));
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
        HealthChanged?.Invoke(Health, maxHealth);
    }

    void Update()   { if (!Derrotado) Machine.LogicUpdate(); }
    void FixedUpdate() { if (!Derrotado) Machine.PhysicsUpdate(); }

    public void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (Derrotado) return;
        if (Machine.CurrentName == "intro") return;   // durante a abertura, intocável

        Health = Mathf.Max(0, Health - amount);
        HealthChanged?.Invoke(Health, maxHealth);
        StartCoroutine(FlashRoutine());

        if (Health <= 0) Machine.ChangeState("morto");
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

    public void AtingirJogadorSeEncostou(float raio)
    {
        var hit = Physics2D.OverlapCircle(transform.position, raio, playerLayer);
        if (hit == null) return;
        var pc = hit.GetComponent<PlayerController>();
        if (pc != null) pc.TakeDamage(danoContato, transform.position);
    }

    /// <summary>
    /// Pisca ao levar golpe. Longo o bastante para aparecer: com 0,08 s o
    /// jogador não via diferença nenhuma e achava que o golpe não tinha entrado.
    /// </summary>
    System.Collections.IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;
        var original = spriteRenderer.color;
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.color = new Color(1f, 0.55f, 0.55f);
            yield return new WaitForSeconds(0.07f);
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = original;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
