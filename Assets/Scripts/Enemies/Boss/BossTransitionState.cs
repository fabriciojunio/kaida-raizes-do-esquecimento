using UnityEngine;

/// <summary>
/// Respiro entre fases: o Guardião fica intocável, limpa o campo e reaparece
/// com o próximo padrão. Dá ao jogador o segundo de leitura que separa
/// "difícil" de "injusto".
/// </summary>
public class BossTransitionState : State
{
    GuardianBoss b;
    float timer;
    const float duracao = 2f;

    public BossTransitionState(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        timer = duracao;
        if (b.Body != null) b.Body.velocity = Vector2.zero;
        b.PlayAnim("hurt");

        // some com os feixes em voo, senão o jogador toma dano durante a pausa
        foreach (var beam in Object.FindObjectsOfType<LumenBeam>())
            Object.Destroy(beam.gameObject);
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) b.AvancarFase();
    }
}
