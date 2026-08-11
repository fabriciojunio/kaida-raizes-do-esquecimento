using UnityEngine;

/// <summary>
/// Javali-Casca (Orla da Vila / Floresta Silente).
/// Morador do vale corrompido pelo Esquecimento: anda devagar, mas ao avistar
/// Kaida ele recua, telegrafa e dispara numa investida reta. Ensina o padrão
/// central do jogo — "leia o aviso, desvie, ataque na abertura".
/// </summary>
public class BoarEnemy : EnemyController
{
    [Header("Investida")]
    public float telegraphTime = 0.45f;    // tempo parado "bufando" antes de correr
    public float chargeSpeed = 8f;
    public float chargeDuration = 0.9f;
    public float recoverTime = 0.7f;       // tonto depois de bater ou terminar

    enum Phase { Rondando, Avisando, Investindo, Recuperando }
    Phase phase = Phase.Rondando;
    float phaseTimer;

    protected override void Update()
    {
        if (dying) return;
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        phaseTimer = Mathf.Max(0f, phaseTimer - Time.deltaTime);

        switch (phase)
        {
            case Phase.Rondando:    TickRondando();    break;
            case Phase.Avisando:    TickAvisando();    break;
            case Phase.Investindo:  TickInvestindo();  break;
            case Phase.Recuperando: TickRecuperando(); break;
        }
    }

    void TickRondando()
    {
        FindPlayer();
        if (target != null)
        {
            SetFacing(target.position.x > transform.position.x ? 1 : -1);
            phase = Phase.Avisando;
            phaseTimer = telegraphTime;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator != null) animator.Play("idle");
            return;
        }
        Patrol();
    }

    void TickAvisando()
    {
        // recua um passo antes de disparar: é o "aviso" que o jogador aprende a ler
        rb.velocity = new Vector2(-facing * moveSpeed * 0.4f, rb.velocity.y);
        if (phaseTimer <= 0f)
        {
            phase = Phase.Investindo;
            phaseTimer = chargeDuration;
            if (animator != null) animator.Play("run");
        }
    }

    void TickInvestindo()
    {
        rb.velocity = new Vector2(facing * chargeSpeed, rb.velocity.y);

        // bateu na parede ou chegou na beirada: para e fica tonto
        if (HasWallAhead() || !HasFloorAhead() || phaseTimer <= 0f)
        {
            phase = Phase.Recuperando;
            phaseTimer = recoverTime;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator != null) animator.Play("idle");
            return;
        }

        // atropela quem estiver no caminho
        var hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            target = hit.transform;
            TryContactAttack();
            phase = Phase.Recuperando;
            phaseTimer = recoverTime;
        }
    }

    void TickRecuperando()
    {
        rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 20f * Time.deltaTime), rb.velocity.y);
        if (phaseTimer <= 0f) phase = Phase.Rondando;
    }

    /// <summary>Levar dano durante a investida interrompe o ataque — é a recompensa por acertar a janela.</summary>
    public override void TakeDamage(int amount, Vector2 sourcePos)
    {
        base.TakeDamage(amount, sourcePos);
        if (!dying && phase == Phase.Investindo)
        {
            phase = Phase.Recuperando;
            phaseTimer = recoverTime;
        }
    }
}
