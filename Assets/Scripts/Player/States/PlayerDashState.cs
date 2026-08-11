using UnityEngine;

/// <summary>Dash horizontal com i-frames, cooldown e limite de dashes no ar.</summary>
public class PlayerDashState : State
{
    PlayerController p;
    float timer;
    public PlayerDashState(PlayerController player) { p = player; }

    public override void Enter()
    {
        timer = p.stats.dashTime;
        p.canDash = false;
        p.dashCooldownTimer = p.stats.dashCooldown;
        p.isInvulnerable = true;
        if (!p.IsGrounded()) p.airDashesLeft--;
        p.SetVelocity(p.facing * p.stats.dashSpeed, 0f);
        p.PlayAnim("dash");
    }

    public override void PhysicsUpdate()
    {
        timer -= Time.fixedDeltaTime;
        p.SetVelocity(p.facing * p.stats.dashSpeed, 0f); // sem gravidade durante o dash
    }

    public override void LogicUpdate()
    {
        if (timer <= 0f)
        {
            p.isInvulnerable = false;
            machine.ChangeState(p.IsGrounded() ? "idle" : "fall");
        }
    }

    public override void Exit() { p.isInvulnerable = false; }
}
