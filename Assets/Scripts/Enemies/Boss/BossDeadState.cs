using UnityEngine;

/// <summary>
/// Fim do confronto. O Guardião se desfaz e o lúmen que ele guardava se
/// solta — o jogo não diz se Kaida o deteve ou se lembrou de si mesma
/// através dele. Fica em aberto, como no GDD.
/// </summary>
public class BossDeadState : State
{
    GuardianBoss b;
    float timer = 3f;
    bool avisou;

    public BossDeadState(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        b.MarcarDerrotado();
        b.PlayAnim("death");

        foreach (var beam in Object.FindObjectsOfType<LumenBeam>())
            Object.Destroy(beam.gameObject);

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkCollected("boss_guardiao");
            SaveSystem.Instance.SaveGame();
        }
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (!avisou && timer <= 1.5f)
        {
            avisou = true;
            MessageUI.Show("O vale volta a lembrar.\n\nFIM", 8f);
        }
    }
}
