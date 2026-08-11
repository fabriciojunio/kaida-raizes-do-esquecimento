using UnityEngine;

/// <summary>Ataque corpo a corpo. Ativa a hitbox numa janela e volta ao estado anterior.</summary>
public class PlayerAttackState : State
{
    PlayerController p;
    float timer;
    const float duration = 0.28f;
    const float hitFrame = 0.10f;
    bool hitApplied;

    public PlayerAttackState(PlayerController player) { p = player; }

    public override void Enter()
    {
        timer = duration;
        hitApplied = false;
        p.PlayAnim("attack");
    }

    public override void PhysicsUpdate()
    {
        p.ApplyGravity(Time.fixedDeltaTime);
        float accel = p.IsGrounded() ? p.stats.groundAccel : p.stats.airAccel;
        float decel = p.IsGrounded() ? p.stats.groundDecel : p.stats.airDecel;
        p.ApplyHorizontal(Time.fixedDeltaTime, p.InputX * 0.3f, accel, decel); // movimento reduzido atacando
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (!hitApplied && timer <= duration - hitFrame)
        {
            p.DoAttackHit();
            hitApplied = true;
        }
        if (timer <= 0f)
        {
            machine.ChangeState(p.IsGrounded() ? (Mathf.Abs(p.InputX) > 0.01f ? "run" : "idle") : "fall");
        }
    }
}
