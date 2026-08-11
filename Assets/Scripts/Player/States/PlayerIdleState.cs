using UnityEngine;

/// <summary>Parado no chão.</summary>
public class PlayerIdleState : State
{
    PlayerController p;
    public PlayerIdleState(PlayerController player) { p = player; }

    public override void Enter()
    {
        p.SetVelocityX(0);
        p.RefreshAirAbilities();
        p.PlayAnim("idle");
    }

    public override void PhysicsUpdate()
    {
        p.ApplyGravity(Time.fixedDeltaTime);
        p.ApplyHorizontal(Time.fixedDeltaTime, 0f, p.stats.groundAccel, p.stats.groundDecel);
    }

    public override void LogicUpdate()
    {
        if (!p.IsGrounded()) { p.coyoteTimer = p.stats.coyoteTime; machine.ChangeState("fall"); return; }
        if (Mathf.Abs(p.InputX) > 0.01f) { machine.ChangeState("run"); return; }
        if (Input.GetButtonDown("Jump")) { machine.ChangeState("jump"); return; }
        if (Input.GetButtonDown("Fire1")) { machine.ChangeState("attack"); return; }
        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash) { machine.ChangeState("dash"); return; }
    }
}
