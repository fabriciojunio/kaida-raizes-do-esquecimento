using UnityEngine;

/// <summary>Subida do pulo — pulo variável (soltar corta a subida).</summary>
public class PlayerJumpState : State
{
    PlayerController p;
    public PlayerJumpState(PlayerController player) { p = player; }

    public override void Enter()
    {
        p.SetVelocityY(p.stats.JumpVelocity);
        p.coyoteTimer = 0f;
        p.PlayAnim("jump");
    }

    public override void PhysicsUpdate()
    {
        p.ApplyGravity(Time.fixedDeltaTime);
        p.ApplyHorizontal(Time.fixedDeltaTime, p.InputX, p.stats.airAccel, p.stats.airDecel);
    }

    public override void LogicUpdate()
    {
        if (Input.GetButtonUp("Jump") && p.rb.velocity.y > 0)
            p.SetVelocityY(p.rb.velocity.y * p.stats.jumpCutMultiplier);

        if (p.rb.velocity.y <= 0) { machine.ChangeState("fall"); return; }
        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash) { machine.ChangeState("dash"); return; }
        if (Input.GetButtonDown("Fire1")) { machine.ChangeState("attack"); return; }
    }
}
