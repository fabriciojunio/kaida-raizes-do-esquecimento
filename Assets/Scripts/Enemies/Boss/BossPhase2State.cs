using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fase 2 - ecos. O Guardião sobe para fora de alcance e chama versões
/// enfraquecidas dos inimigos que Kaida já venceu.
///
/// Limpa a onda, ele desce até a altura de quem está jogando e fica exposto
/// até chamar a próxima. Antes ele apenas parava de subir, e como o ataque da
/// Kaida é corpo a corpo a fase não tinha como terminar: era possível matar
/// todos os ecos e mesmo assim não alcançar o chefe.
/// </summary>
public class BossPhase2State : State
{
    GuardianBoss b;
    float timerOnda;
    bool ondaEmCampo;
    readonly List<EnemyController> ecos = new List<EnemyController>();

    /// <summary>Altura de recuo enquanto os ecos estão em campo.</summary>
    const float AlturaDeRecuo = 6f;

    public BossPhase2State(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        b.PlayAnim("idle");
        timerOnda = 0.8f;
        ondaEmCampo = false;
        ecos.Clear();
        MessageUI.Show("Ele chama os ecos de quem você já enfrentou.", 3f);
    }

    public override void PhysicsUpdate()
    {
        if (b.Body == null) return;

        float vx = Mathf.Sin(Time.time * 0.5f) * 1.2f;

        // Com ecos em campo ele se afasta; sem ecos, desce até ficar ao
        // alcance da espada. É a janela de dano da fase.
        float alvoY = b.player != null
            ? b.player.position.y + (ondaEmCampo ? AlturaDeRecuo : 1.4f)
            : b.transform.position.y;

        float diferenca = alvoY - b.transform.position.y;
        float vy = Mathf.Clamp(diferenca, -2.4f, 2.4f) * (ondaEmCampo ? 0.9f : 1.6f);

        b.Body.velocity = new Vector2(vx, vy);
    }

    public override void LogicUpdate()
    {
        timerOnda -= Time.deltaTime;

        if (!ondaEmCampo && timerOnda <= 0f)
        {
            int criados = b.InvocarEcos(b.ecosPorOnda, ecos);
            ondaEmCampo = criados > 0;
            // sem prefabs de eco configurados a fase travaria: segue no tempo
            timerOnda = b.intervaloEntreOndas;
            b.PlayAnim("attack");
            return;
        }

        if (ondaEmCampo && !AlgumEcoVivo())
        {
            // onda limpa: ele desce e fica atingível até chamar a próxima
            ondaEmCampo = false;
            ecos.Clear();
            timerOnda = b.intervaloEntreOndas * 0.5f;
            MessageUI.Show("Sem os ecos, ele desce. É agora.", 2f);
        }
    }

    /// <summary>
    /// Só os ecos desta onda contam. Varrer a cena inteira atrás de inimigos
    /// prendia a fase para sempre em qualquer arena que tivesse um inimigo
    /// comum parado num canto.
    /// </summary>
    bool AlgumEcoVivo()
    {
        for (int i = ecos.Count - 1; i >= 0; i--)
        {
            var e = ecos[i];
            if (e == null || e.Dying) { ecos.RemoveAt(i); continue; }
            return true;
        }
        return false;
    }

    public override void Exit()
    {
        ecos.Clear();
    }
}
