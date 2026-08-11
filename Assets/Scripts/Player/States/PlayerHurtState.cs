using UnityEngine;

/// <summary>Estado de dano: aplica knockback e i-frames.</summary>
public class PlayerHurtState : State
{
    PlayerController p;
    float timer;
    const float hurtTime = 0.25f;

    public PlayerHurtState(PlayerController player) { p = player; }

    public override void Enter()
    {
        timer = hurtTime;
        p.isInvulnerable = true;
        p.SetVelocity(p.pendingKnockbackDir * p.stats.knockbackForce, p.stats.knockbackForce * 0.5f);
        p.PlayAnim("hurt");
    }

    public override void PhysicsUpdate()
    {
        timer -= Time.fixedDeltaTime;
        p.ApplyGravity(Time.fixedDeltaTime);
        p.SetVelocityX(Mathf.MoveTowards(p.rb.velocity.x, 0f, p.stats.groundDecel * Time.fixedDeltaTime));
    }

    public override void LogicUpdate()
    {
        if (timer <= 0f)
        {
            // liga a janela ANTES de trocar de estado: o Enter do próximo estado
            // não pode rodar com o jogador momentaneamente vulnerável
            p.StartInvulnWindow();
            machine.ChangeState(p.IsGrounded() ? "idle" : "fall");
        }
    }
}
