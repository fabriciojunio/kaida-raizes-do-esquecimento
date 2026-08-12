using UnityEngine;

/// <summary>
/// Abelha-Eco (Caverna Musgosa). Voadora: ignora gravidade, patrulha numa
/// onda senoidal e mergulha em diagonal sobre Kaida. Obriga o jogador a olhar
/// para cima, não só para os lados - o conceito novo da Caverna.
/// </summary>
public class BeeEnemy : EnemyController
{
    [Header("Voo")]
    public float amplitude = 0.7f;      // altura da ondulação
    public float frequency = 2.2f;      // velocidade da ondulação
    public float horizontalRange = 3f;  // quanto vagueia para os lados

    [Header("Mergulho")]
    public float diveSpeed = 9f;
    public float diveCooldown = 2.2f;
    public float returnSpeed = 4f;

    [Header("Janela de contra-ataque")]
    [Tooltip("Tempo pairando na altura do jogador depois do mergulho. É a " +
             "janela em que dá para revidar - sem ela a abelha volta para o " +
             "alto e o ataque corpo a corpo nunca alcança.")]
    public float tempoPairando = 1.1f;

    enum Phase { Voando, Mergulhando, Pairando, Voltando }
    Phase phase = Phase.Voando;
    Vector3 anchor;        // ponto de voo (altura de descanso)
    Vector2 diveTarget;
    float diveTimer;
    float pairarTimer;
    float wanderDir = 1f;

    protected override void Awake()
    {
        base.Awake();
        anchor = transform.position;
        rb.gravityScale = 0f;       // voa: gravidade não se aplica
        rb.freezeRotation = true;
    }

    protected override void Update()
    {
        if (dying) return;
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        diveTimer = Mathf.Max(0f, diveTimer - Time.deltaTime);

        switch (phase)
        {
            case Phase.Voando:      TickVoando();      break;
            case Phase.Mergulhando: TickMergulhando(); break;
            case Phase.Pairando:    TickPairando();    break;
            case Phase.Voltando:    TickVoltando();    break;
        }
    }

    /// <summary>Fica quase parada, na altura em que chegou, dando a brecha.</summary>
    void TickPairando()
    {
        if (animator != null) animator.Play("fly");

        pairarTimer -= Time.deltaTime;
        rb.velocity = new Vector2(Mathf.Sin(Time.time * 3f) * 0.5f,
                                  Mathf.Cos(Time.time * 4f) * 0.35f);

        var hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null) { target = hit.transform; TryContactAttack(); }

        if (pairarTimer <= 0f)
        {
            phase = Phase.Voltando;
            diveTimer = diveCooldown;
        }
    }

    void TickVoando()
    {
        if (animator != null) animator.Play("fly");

        // vagueia em torno da âncora, ondulando
        if (Mathf.Abs(transform.position.x - anchor.x) > horizontalRange) wanderDir = -wanderDir;
        float vy = Mathf.Cos(Time.time * frequency) * amplitude * frequency;
        rb.velocity = new Vector2(wanderDir * moveSpeed, vy);
        SetFacing(wanderDir > 0 ? 1 : -1);

        FindPlayer();
        if (target != null && diveTimer <= 0f)
        {
            diveTarget = target.position;
            SetFacing(diveTarget.x > transform.position.x ? 1 : -1);
            phase = Phase.Mergulhando;
            if (animator != null) animator.Play("attack");
        }
    }

    void TickMergulhando()
    {
        Vector2 dir = (diveTarget - (Vector2)transform.position);
        if (dir.magnitude < 0.35f || HasWallAhead())
        {
            phase = Phase.Pairando;              // fica ao alcance por um tempo
            pairarTimer = tempoPairando;
            return;
        }
        rb.velocity = dir.normalized * diveSpeed;

        var hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            target = hit.transform;
            TryContactAttack();
            phase = Phase.Voltando;
            diveTimer = diveCooldown;
        }
    }

    void TickVoltando()
    {
        if (animator != null) animator.Play("fly");
        Vector2 dir = (anchor - transform.position);
        if (dir.magnitude < 0.3f) { phase = Phase.Voando; return; }
        rb.velocity = dir.normalized * returnSpeed;
    }

    /// <summary>
    /// Ao ser ferida ela recua, mas sem subir para fora de alcance: o
    /// contra-ataque precisa continuar possível. Antes ela voltava direto
    /// para a âncora, alto demais para o golpe corpo a corpo alcançar.
    /// </summary>
    public override void TakeDamage(int amount, Vector2 sourcePos)
    {
        base.TakeDamage(amount, sourcePos);
        if (dying) return;

        phase = Phase.Voltando;
        diveTimer = Mathf.Min(diveTimer, 0.6f);
    }

    /// <summary>Voadora não tem chão para checar: nunca "cai" da plataforma.</summary>
    protected override void Patrol() { }
}
