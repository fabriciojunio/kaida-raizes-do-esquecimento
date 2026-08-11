using UnityEngine;

/// <summary>
/// Abelha-Eco (Caverna Musgosa). Voadora: ignora gravidade, patrulha numa
/// onda senoidal e mergulha em diagonal sobre Kaida. Obriga o jogador a olhar
/// para cima, não só para os lados — o conceito novo da Caverna.
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

    enum Phase { Voando, Mergulhando, Voltando }
    Phase phase = Phase.Voando;
    Vector3 anchor;        // ponto de voo (altura de descanso)
    Vector2 diveTarget;
    float diveTimer;
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
            case Phase.Voltando:    TickVoltando();    break;
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
            phase = Phase.Voltando;
            diveTimer = diveCooldown;
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

    /// <summary>Voadora não tem chão para checar: nunca "cai" da plataforma.</summary>
    protected override void Patrol() { }
}
