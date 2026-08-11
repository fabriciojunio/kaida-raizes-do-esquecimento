using UnityEngine;

/// <summary>Queda — com coyote time e jump buffering.</summary>
public class PlayerFallState : State
{
    PlayerController p;
    public PlayerFallState(PlayerController player) { p = player; }

    public override void Enter() { p.PlayAnim("fall"); }

    public override void PhysicsUpdate()
    {
        p.ApplyGravity(Time.fixedDeltaTime);
        p.ApplyHorizontal(Time.fixedDeltaTime, p.InputX, p.stats.airAccel, p.stats.airDecel);
    }

    public override void LogicUpdate()
    {
        if (Input.GetButtonDown("Jump")) p.BufferJump();

        if (p.IsGrounded())
        {
            p.RefreshAirAbilities();
            if (p.ConsumeJumpBuffer()) { machine.ChangeState("jump"); return; }
            machine.ChangeState(Mathf.Abs(p.InputX) > 0.01f ? "run" : "idle");
            return;
        }
        if (Input.GetButtonDown("Jump") && p.CanCoyoteJump()) { machine.ChangeState("jump"); return; }
        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash) { machine.ChangeState("dash"); return; }
        if (Input.GetButtonDown("Fire1")) { machine.ChangeState("attack"); return; }
    }
}
