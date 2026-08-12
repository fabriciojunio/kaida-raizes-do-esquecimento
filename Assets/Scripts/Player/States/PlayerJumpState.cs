using UnityEngine;

/// <summary>Subida do pulo - pulo variável (soltar corta a subida).</summary>
public class PlayerJumpState : State
{
    PlayerController p;
    public PlayerJumpState(PlayerController player) { p = player; }

    public override void Enter()
    {
        // pulo aéreo (double_jump) sai um pouco mais fraco que o pulo do chão
        float power = p.pendingAirJump ? p.stats.airJumpPower : 1f;
        p.pendingAirJump = false;
        p.SetVelocityY(p.stats.JumpVelocity * power);
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

        // segundo pulo no ar, sem sair do estado (só reaplica a força)
        if (Input.GetButtonDown("Jump") && p.CanAirJump())
        {
            p.ConsumeAirJump();
            Enter();
            return;
        }
        if (p.rb.velocity.y <= 0) { machine.ChangeState("fall"); return; }
        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash && p.airDashesLeft > 0) { machine.ChangeState("dash"); return; }
        if (Input.GetButtonDown("Fire1")) { machine.ChangeState("attack"); return; }
    }
}
