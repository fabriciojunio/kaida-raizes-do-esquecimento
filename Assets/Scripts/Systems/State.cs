/// <summary>
/// Base de um estado do jogador/inimigo. Sobrescreva os métodos necessários.
/// </summary>
public abstract class State
{
    protected StateMachine machine;
    public void SetMachine(StateMachine m) { machine = m; }

    public virtual void Enter() {}
    public virtual void Exit() {}
    public virtual void LogicUpdate() {}       // chamado em Update()
    public virtual void PhysicsUpdate() {}     // chamado em FixedUpdate()
}
