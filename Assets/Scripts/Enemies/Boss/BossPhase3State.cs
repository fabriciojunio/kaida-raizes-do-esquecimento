using UnityEngine;

/// <summary>
/// Fase 3 - corpo a corpo. Ele desce e persegue Kaida em investidas rápidas,
/// alternando com feixes curtos para fechar as rotas de fuga. É onde dash,
/// pulo duplo e parede precisam ser usados juntos para não ser encurralada.
/// </summary>
public class BossPhase3State : State
{
    GuardianBoss b;
    float timerInvestida;
    bool investindo;
    Vector2 alvoInvestida;

    public BossPhase3State(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        b.PlayAnim("idle");
        timerInvestida = 0.9f;
        investindo = false;
        MessageUI.Show("Não resta mais distância entre vocês.", 3f);
    }

    public override void PhysicsUpdate()
    {
        if (b.Body == null) return;

        if (investindo)
        {
            Vector2 dir = (alvoInvestida - (Vector2)b.transform.position);
            if (dir.magnitude < 0.4f)
            {
                investindo = false;
                timerInvestida = b.intervaloInvestida;
                b.Body.velocity = Vector2.zero;
                b.PlayAnim("idle");
            }
            else
            {
                b.Body.velocity = dir.normalized * b.velocidadeInvestida;
            }
        }
        else if (b.player != null)
        {
            // ronda perto do jogador, sem encostar
            Vector2 para = ((Vector2)b.player.position - (Vector2)b.transform.position);
            float dist = para.magnitude;
            Vector2 v = dist > 3.5f ? para.normalized * 2.2f : -para.normalized * 1.2f;
            b.Body.velocity = v;
        }
    }

    public override void LogicUpdate()
    {
        b.AtingirJogadorSeEncostou(0.7f);

        if (investindo) return;

        timerInvestida -= Time.deltaTime;
        if (timerInvestida <= 0f && b.player != null)
        {
            alvoInvestida = b.player.position;
            investindo = true;
            b.PlayAnim("attack");

            // feixe de acompanhamento fecha a rota de escape lateral
            Vector2 dir = (alvoInvestida - (Vector2)b.transform.position).normalized;
            b.DispararFeixe(Quaternion.Euler(0f, 0f, 90f) * dir);
        }
    }
}
