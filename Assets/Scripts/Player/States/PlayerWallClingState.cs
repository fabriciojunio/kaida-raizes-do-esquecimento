using UnityEngine;

/// <summary>
/// Agarrado na parede (habilidade "wall_climb", obtida no fim da Caverna Musgosa).
/// Kaida desliza devagar para baixo e pode saltar para o lado oposto. É o que
/// abre os atalhos verticais de volta para a Vila e a Floresta.
/// </summary>
public class PlayerWallClingState : State
{
    PlayerController p;
    int wallSide;   // lado da parede: 1 = parede à direita, -1 = à esquerda

    public PlayerWallClingState(PlayerController player) { p = player; }

    public override void Enter()
    {
        wallSide = p.facing;
        p.SetVelocity(0f, 0f);
        p.RefreshAirAbilities();   // agarrar na parede devolve dash e pulo duplo
        p.PlayAnim("wallcling");
    }

    public override void PhysicsUpdate()
    {
        // desliza para baixo num ritmo constante, em vez da gravidade cheia
        p.SetVelocity(0f, -p.stats.wallSlideSpeed);
    }

    public override void LogicUpdate()
    {
        if (p.IsGrounded()) { machine.ChangeState("idle"); return; }

        // soltou a direção ou a parede acabou
        if (!p.IsTouchingWall() || Mathf.Abs(p.InputX) < 0.01f || Mathf.Sign(p.InputX) != wallSide)
        {
            machine.ChangeState("fall");
            return;
        }

        if (Input.GetButtonDown("Jump"))
        {
            // salta para o lado oposto da parede e trava o controle por um instante
            p.SetFacing(-wallSide);
            p.SetVelocity(-wallSide * p.stats.wallJumpForceX, p.stats.JumpVelocity * p.stats.wallJumpPower);
            p.wallJumpLockTimer = p.stats.wallJumpLockTime;
            p.PlayAnim("jump");
            machine.ChangeState("fall");   // o fall assume a subida com o controle travado
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash && p.airDashesLeft > 0)
        {
            p.SetFacing(-wallSide);
            machine.ChangeState("dash");
            return;
        }
    }

    public override void Exit()
    {
        p.wallJumpLockTimer = Mathf.Max(p.wallJumpLockTimer, 0f);
    }
}
