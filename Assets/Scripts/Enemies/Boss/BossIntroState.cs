using UnityEngine;

/// <summary>Abertura: o Guardião desperta. Alguns segundos sem ataque, para
/// o jogador ver a criatura antes de ser cobrado por ela.</summary>
public class BossIntroState : State
{
    GuardianBoss b;
    float timer;

    /// <summary>
    /// Tempo para ler a criatura antes de ser cobrado por ela. Os 2,2s
    /// iniciais deixavam o combate começar antes de o jogador se posicionar.
    /// Público para os testes esperarem o tempo certo em vez de repetir o
    /// número e sair de sincronia quando ele mudar.
    /// </summary>
    public const float Duracao = 4.5f;

    public BossIntroState(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        timer = Duracao;
        b.PlayAnim("idle");
        if (b.Body != null) b.Body.velocity = Vector2.zero;
        MessageUI.Show("O Guardião do Lúmen desperta.", 2.5f);
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) machine.ChangeState("fase1");
    }
}
