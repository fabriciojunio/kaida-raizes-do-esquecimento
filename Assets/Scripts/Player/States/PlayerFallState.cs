using UnityEngine;

/// <summary>Queda - com coyote time e jump buffering.</summary>
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
        bool jumpPressed = Input.GetButtonDown("Jump");
        if (jumpPressed) p.BufferJump();

        if (p.IsGrounded())
        {
            p.RefreshAirAbilities();
            if (p.ConsumeJumpBuffer()) { machine.ChangeState("jump"); return; }
            machine.ChangeState(Mathf.Abs(p.InputX) > 0.01f ? "run" : "idle");
            return;
        }
        if (jumpPressed)
        {
            // coyote time tem prioridade: ainda conta como pulo "do chão"
            if (p.CanCoyoteJump()) { p.ConsumeJumpBuffer(); machine.ChangeState("jump"); return; }
            if (p.CanAirJump()) { p.ConsumeJumpBuffer(); p.ConsumeAirJump(); machine.ChangeState("jump"); return; }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash && p.airDashesLeft > 0) { machine.ChangeState("dash"); return; }
        if (Input.GetButtonDown("Fire1")) { machine.ChangeState("attack"); return; }
    }
}
