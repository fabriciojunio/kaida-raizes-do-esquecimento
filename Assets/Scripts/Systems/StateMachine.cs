using System.Collections.Generic;

/// <summary>
/// Máquina de estados simples e reutilizável. Registre os estados por nome e
/// troque com ChangeState("nome").
/// </summary>
public class StateMachine
{
    public State Current { get; private set; }
    public string CurrentName { get; private set; } = "";
    readonly Dictionary<string, State> states = new Dictionary<string, State>();

    public void Add(string name, State state)
    {
        state.SetMachine(this);
        states[name.ToLower()] = state;
    }

    public void SetInitial(string name)
    {
        string key = name.ToLower();
        if (!states.ContainsKey(key)) return;
        Current = states[key];
        CurrentName = key;
        Current.Enter();
    }

    public void ChangeState(string name)
    {
        string key = name.ToLower();
        if (!states.ContainsKey(key) || states[key] == Current) return;
        Current?.Exit();
        Current = states[key];
        CurrentName = key;
        Current.Enter();
    }

    public void LogicUpdate() { Current?.LogicUpdate(); }
    public void PhysicsUpdate() { Current?.PhysicsUpdate(); }
}
