using UnityEngine;

/// <summary>
/// Fase 1 - feixes de memória. O Guardião flutua devagar e dispara salvas
/// em leque na direção de Kaida. Cobra o dash: dá para atravessar a salva
/// na janela de invulnerabilidade.
/// </summary>
public class BossPhase1State : State
{
    GuardianBoss b;
    float timerSalva;
    float tempoNoEstado;

    public BossPhase1State(GuardianBoss boss) { b = boss; }

    public override void Enter()
    {
        timerSalva = 1f;
        tempoNoEstado = 0f;
        b.PlayAnim("idle");
    }

    public override void PhysicsUpdate()
    {
        if (b.Body == null) return;

        // Vaivém lento que desce até a altura do jogador de tempos em tempos.
        // Ficando sempre no alto, ele era intocável: o ataque da Kaida é
        // corpo a corpo e o confronto não saía do lugar.
        float vx = Mathf.Sin(tempoNoEstado * 0.7f) * 1.8f;
        float vy = Mathf.Cos(tempoNoEstado * 1.1f) * 0.6f;

        if (b.player != null)
        {
            float diferenca = b.player.position.y + 1.5f - b.transform.position.y;
            // aproxima devagar da altura de quem está jogando
            vy += Mathf.Clamp(diferenca, -1.6f, 1.6f) * 0.8f;
        }

        b.Body.velocity = new Vector2(vx, vy);
    }

    public override void LogicUpdate()
    {
        tempoNoEstado += Time.deltaTime;
        timerSalva -= Time.deltaTime;

        if (timerSalva <= 0f)
        {
            DispararSalva();
            timerSalva = b.beamInterval;
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
