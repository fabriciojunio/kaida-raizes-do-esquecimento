using UnityEngine;

/// <summary>
/// O confronto, do começo ao fim. Sem fases e sem invocação: o Guardião
/// alterna feixes à distância e investidas de perto, sempre na altura da
/// Kaida, para que a espada sempre tenha como alcançá-lo.
///
/// Ficar no alto, como ele ficava antes, o tornava intocável: o ataque da
/// Kaida é corpo a corpo e o combate não saía do lugar.
/// </summary>
public class BossCombateState : State
{
    GuardianBoss b;

    float timerSalva;
    float timerInvestida;
    float tempoNoEstado;

    bool investindo;
    Vector2 alvoInvestida;

    public BossCombateState(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        timerSalva = 1.5f;
        timerInvestida = b.intervaloInvestida;
        tempoNoEstado = 0f;
        investindo = false;
        b.PlayAnim("idle");
    }

    public override void PhysicsUpdate()
    {
        if (b.Body == null) return;

        if (investindo)
        {
            Vector2 rumo = alvoInvestida - (Vector2)b.transform.position;
            if (rumo.magnitude < 0.5f)
            {
                investindo = false;
                timerInvestida = b.intervaloInvestida;
                b.Body.velocity = Vector2.zero;
                b.PlayAnim("idle");
            }
            else
            {
                b.Body.velocity = rumo.normalized * b.velocidadeInvestida;
            }
            return;
        }

        // vaivém lento acompanhando a altura do jogador
        float vx = Mathf.Sin(tempoNoEstado * 0.7f) * 1.8f;
        float vy = 0f;

        if (b.player != null)
        {
            float diferenca = b.player.position.y + 1.4f - b.transform.position.y;
            vy = Mathf.Clamp(diferenca, -2f, 2f) * 1.4f;

            // não cola no jogador: mantém uma distância de leitura
            float dx = b.player.position.x - b.transform.position.x;
            if (Mathf.Abs(dx) > 4f) vx += Mathf.Sign(dx) * 1.6f;
            else if (Mathf.Abs(dx) < 1.5f) vx -= Mathf.Sign(dx) * 1.2f;
        }

        b.Body.velocity = new Vector2(vx, vy);
    }

    public override void LogicUpdate()
    {
        tempoNoEstado += Time.deltaTime;
        b.AtingirJogadorSeEncostou(0.9f);

        if (investindo) return;

        timerSalva -= Time.deltaTime;
        if (timerSalva <= 0f)
        {
            DispararSalva();
            timerSalva = b.beamInterval;
        }

        timerInvestida -= Time.deltaTime;
        if (timerInvestida <= 0f && b.player != null)
        {
            alvoInvestida = b.player.position;
            investindo = true;
            b.PlayAnim("attack");
        }
    }

    void DispararSalva()
    {
        if (b.player == null) return;
        b.PlayAnim("attack");

        Vector2 baseDir = ((Vector2)b.player.position - (Vector2)b.transform.position).normalized;
        int n = Mathf.Max(1, b.beamsPorSalva);
        float aberturaTotal = 26f;                       // leque em graus
        float passo = n > 1 ? aberturaTotal / (n - 1) : 0f;
        float inicio = -aberturaTotal * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float ang = inicio + passo * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, ang) * baseDir;
            b.DispararFeixe(dir);
        }
    }
}
