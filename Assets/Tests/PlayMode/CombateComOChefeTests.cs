using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Confronto com o Guardião na cena de verdade.
///
/// Os testes anteriores montavam um chefe simplificado à mão e passavam,
/// enquanto no jogo o golpe não acertava. A diferença estava no prefab real:
/// escala, colisor e altura de voo. Aqui a cena é a mesma que o jogador abre.
/// </summary>
public class CombateComOChefeTests
{
    [TearDown]
    public void Depois()
    {
        // Estes testes abrem as regiões de verdade. Sem limpar, os inimigos e
        // o Guardião continuam vivos e atrapalham quem roda depois.
        Time.timeScale = 1f;
        CenarioDeTeste.Limpar();
    }

    [UnityTest]
    public IEnumerator Kaida_ConsegueFerirOChefeEncostada()
    {
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        var kaida = Object.FindObjectOfType<PlayerController>();
        Assert.IsNotNull(boss, "o Guardião não está na cena final");
        Assert.IsNotNull(kaida, "a Kaida não está na cena final");

        // passa a abertura, quando ele é intocável de propósito
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.5f);
        Assert.AreEqual("combate", boss.Machine.CurrentName);

        kaida.transform.position = boss.transform.position + new Vector3(-1.8f, -0.6f, 0f);
        kaida.SetFacing(1);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        var colisorDoChefe = boss.GetComponent<Collider2D>();
        Assert.IsNotNull(colisorDoChefe, "o Guardião está sem colisor");

        var area = new Vector2(kaida.attackRadius * 2f, kaida.attackRadius * 2.6f);
        var atingidos = Physics2D.OverlapBoxAll(
            kaida.PontoAFrente(kaida.attackPoint), area, 0f, kaida.enemyLayer);

        Assert.Greater(atingidos.Length, 0,
            $"o golpe não alcança o chefe. Kaida em {kaida.transform.position}, " +
            $"colisor do chefe em {colisorDoChefe.bounds.center} " +
            $"medindo {colisorDoChefe.bounds.size}, " +
            $"máscara de inimigo {kaida.enemyLayer.value}, " +
            $"camada do chefe {boss.gameObject.layer}");

        int vidaAntes = boss.Health;
        kaida.DoAttackHit();
        yield return null;

        Assert.Less(boss.Health, vidaAntes,
            "o Guardião não recebeu dano mesmo com a Kaida encostada nele");
    }

    [UnityTest]
    public IEnumerator OChefe_MorreEVenceOJogo()
    {
        // O teste mais importante do jogo: dá para terminar. Bate no Guardião
        // até a barra zerar, sem tocar em mais nada da arena.
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        var kaida = Object.FindObjectOfType<PlayerController>();
        Assert.IsNotNull(boss);

        bool venceu = false;
        boss.Morreu += () => venceu = true;

        kaida.isInvulnerable = true;   // aqui se mede o dano dado, não o sofrido
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.5f);

        float limite = 60f, gasto = 0f;
        while (!boss.Derrotado && gasto < limite)
        {
            kaida.transform.position = boss.transform.position + new Vector3(-1.7f, -0.5f, 0f);
            kaida.SetFacing(1);
            yield return new WaitForFixedUpdate();
            kaida.DoAttackHit();

            for (float t = 0f; t < 0.25f; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                gasto += Time.fixedDeltaTime;
            }
        }

        Assert.IsTrue(boss.Derrotado,
            $"o Guardião não caiu em {limite:F0} segundos de combate. " +
            $"Parou com {boss.Health} de {boss.maxHealth} de vida, " +
            $"estado {boss.Machine.CurrentName}.");

        yield return null;
        Assert.IsTrue(venceu, "a morte do chefe não avisou ninguém: não há vitória");
    }

    [UnityTest]
    public IEnumerator OChefe_NaoInvocaInimigos()
    {
        // A arena tem três inimigos comuns, colocados no mapa. O chefe não
        // repõe nem chama mais: o alvo do confronto é ele, não a horda.
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        int noInicio = Object.FindObjectsOfType<EnemyController>().Length;

        yield return new WaitForSeconds(BossIntroState.Duracao + 6f);

        int agora = Object.FindObjectsOfType<EnemyController>().Length;
        Assert.LessOrEqual(agora, noInicio,
            $"a arena tinha {noInicio} inimigos e passou a ter {agora}: " +
            "o chefe está invocando gente.");
    }

    [UnityTest]
    public IEnumerator Chefe_DesceAteAAlturaDoJogador()
    {
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        var kaida = Object.FindObjectOfType<PlayerController>();

        yield return new WaitForSeconds(BossIntroState.Duracao + 0.5f);

        // acompanha por alguns segundos e guarda a menor distância vertical
        float melhorDistancia = float.MaxValue;
        float velocidadeMaxima = 0f;
        for (float t = 0f; t < 6f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            float d = boss.transform.position.y - kaida.transform.position.y;
            melhorDistancia = Mathf.Min(melhorDistancia, Mathf.Abs(d));
            if (boss.Body != null)
                velocidadeMaxima = Mathf.Max(velocidadeMaxima, Mathf.Abs(boss.Body.velocity.y));
        }

        Assert.Less(melhorDistancia, 4f,
            $"em 6 segundos o chefe nunca chegou perto: melhor aproximação foi " +
            $"{melhorDistancia:F1} unidades acima do jogador. " +
            $"Estado: {boss.Machine.CurrentName}, " +
            $"alvo definido: {(boss.player != null)}, " +
            $"corpo: {(boss.Body != null ? boss.Body.bodyType.ToString() : "sem Rigidbody")}, " +
            $"maior velocidade vertical: {velocidadeMaxima:F2}. " +
            "Ficando sempre no alto, ele é intocável para um ataque corpo a corpo.");
    }
}
