using UnityEngine;

/// <summary>
/// Fase 2 — ecos. O Guardião sobe para fora de alcance e chama versões
/// enfraquecidas dos inimigos que Kaida já venceu. Ele só volta a ser
/// atingível quando a onda é limpa: a fase testa o combate aprendido,
/// não o dano no chefe.
/// </summary>
public class BossPhase2State : State
{
    GuardianBoss b;
    float timerOnda;
    bool ondaEmCampo;

    public BossPhase2State(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        b.PlayAnim("idle");
        timerOnda = 0.8f;
        ondaEmCampo = false;
        MessageUI.Show("Ele chama os ecos de quem você já enfrentou.", 3f);
    }

    public override void PhysicsUpdate()
    {
        // recua para o alto enquanto os ecos lutam por ele
        if (b.Body != null)
            b.Body.velocity = new Vector2(Mathf.Sin(Time.time * 0.5f) * 1.2f, 0f);
    }

    public override void LogicUpdate()
    {
        timerOnda -= Time.deltaTime;

        if (!ondaEmCampo && timerOnda <= 0f)
        {
            int criados = b.InvocarEcos(b.ecosPorOnda);
            ondaEmCampo = criados > 0;
            // sem prefabs de eco configurados a fase travaria: segue no tempo
            timerOnda = b.intervaloEntreOndas;
            b.PlayAnim("attack");
            return;
        }

        if (ondaEmCampo && !b.AindaTemEcos())
        {
            // onda limpa: janela para bater no chefe até ele chamar a próxima
            ondaEmCampo = false;
            timerOnda = b.intervaloEntreOndas * 0.5f;
        }
    }
}
