using UnityEngine;

/// <summary>Abertura: o Guardião desperta. Alguns segundos sem ataque, para
/// o jogador ver a criatura antes de ser cobrado por ela.</summary>
public class BossIntroState : State
{
    GuardianBoss b;
    float timer;
    const float duracao = 2.2f;

    public BossIntroState(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        timer = duracao;
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
