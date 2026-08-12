using UnityEngine;

/// <summary>
/// Agarrada na parede (habilidade "wall_climb", obtida na Caverna Musgosa).
/// Kaida desliza devagar e salta para o lado oposto. É o que vence o poço de
/// saída da Caverna e abre os atalhos verticais de volta.
/// </summary>
public class PlayerWallClingState : State
{
    PlayerController p;
    int wallSide;      // lado da parede: 1 = parede à direita, -1 = à esquerda
    float carencia;

    /// <summary>
    /// Tempo que ela continua "agarrada" depois de perder o contato ou de o
    /// jogador soltar a direção.
    ///
    /// É o coyote time da parede, e é o que separa uma escalada que responde
    /// de uma que parece travada: sem ele, qualquer folga de um frame entre
    /// encostar e apertar pulo virava queda até o pé do poço.
    /// </summary>
    const float Carencia = 0.12f;

    public PlayerWallClingState(PlayerController player) { p = player; }

    public override void Enter()
    {
        wallSide = p.LadoDaParede();
        if (wallSide == 0) wallSide = p.facing;

        p.SetFacing(wallSide);     // encara a parede, senão o sprite fica de costas
        p.SetVelocity(0f, 0f);
        p.RefreshAirAbilities();   // agarrar devolve o dash e o pulo duplo
        p.PlayAnim("wallcling");
        carencia = Carencia;
    }

    public override void PhysicsUpdate()
    {
        // desliza para baixo num ritmo constante, em vez da gravidade cheia
        p.SetVelocity(0f, -p.stats.wallSlideSpeed);
    }

    public override void LogicUpdate()
    {
        if (p.IsGrounded()) { machine.ChangeState("idle"); return; }

        if (Input.GetButtonDown("Jump"))
        {
            Saltar();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && p.canDash && p.airDashesLeft > 0)
        {
            p.SetFacing(-wallSide);
            machine.ChangeState("dash");
            return;
        }

        bool naParede = p.LadoDaParede() == wallSide;
        bool empurrandoParaLonge = Mathf.Abs(p.InputX) > 0.01f && Mathf.Sign(p.InputX) != wallSide;

        if (naParede && !empurrandoParaLonge)
        {
            carencia = Carencia;
            return;
        }

        carencia -= Time.deltaTime;
        if (carencia <= 0f) machine.ChangeState("fall");
    }

    void Saltar()
    {
        // salta para o lado oposto e trava o controle por um instante, senão
        // segurar a direção da parede colava a Kaida de volta nela
        p.SetFacing(-wallSide);
        p.SetVelocity(-wallSide * p.stats.wallJumpForceX, p.stats.JumpVelocity * p.stats.wallJumpPower);
        p.wallJumpLockTimer = p.stats.wallJumpLockTime;
        p.PlayAnim("jump");
        machine.ChangeState("fall");   // o fall assume a subida com o controle travado
    }
}
