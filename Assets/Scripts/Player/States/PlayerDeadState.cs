using UnityEngine;

/// <summary>Morte: zera velocidade e trava o corpo. Respawn é tratado pelo GameManager.</summary>
public class PlayerDeadState : State
{
    PlayerController p;
    public PlayerDeadState(PlayerController player) { p = player; }

    public override void Enter()
    {
        p.SetVelocity(0, 0);
        p.isInvulnerable = true;
        p.PlayAnim("death");
    }

    public override void PhysicsUpdate() { /* corpo parado */ }
    public override void LogicUpdate() { /* aguarda GameManager chamar respawn */ }
}
