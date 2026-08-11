using UnityEngine;

/// <summary>
/// Caracol-Rastejante (Caverna Musgosa). Lento e inofensivo de longe, mas se
/// esconde na casca quando atacado e fica imune por um tempo. Não dá para
/// resolver no botão: o jogador precisa esperar a casca abrir.
/// </summary>
public class SnailEnemy : EnemyController
{
    [Header("Casca")]
    public float hideDuration = 1.6f;      // tempo escondido (imune)
    public int hitsBeforeHiding = 1;       // golpes aceitos antes de se fechar

    bool hidden = false;
    float hideTimer;
    int hitsTaken;

    protected override void Update()
    {
        if (dying) return;

        if (hidden)
        {
            hideTimer -= Time.deltaTime;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (hideTimer <= 0f)
            {
                hidden = false;
                hitsTaken = 0;
                if (animator != null) animator.Play("walk");
            }
            return;
        }

        base.Update();
    }

    public override void TakeDamage(int amount, Vector2 sourcePos)
    {
        // dentro da casca não entra dano: o jogador precisa ler o timing
        if (hidden || dying) return;

        base.TakeDamage(amount, sourcePos);
        if (dying) return;

        hitsTaken++;
        if (hitsTaken >= hitsBeforeHiding)
        {
            hidden = true;
            hideTimer = hideDuration;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator != null) animator.Play("hide");
        }
    }

    /// <summary>Escondido não causa dano de contato.</summary>
    protected override void ChaseAndAttack()
    {
        if (hidden) return;
        base.ChaseAndAttack();
    }

    public bool IsHidden => hidden;
}
