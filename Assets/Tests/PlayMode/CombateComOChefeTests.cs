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
        Assert.AreEqual("fase1", boss.Machine.CurrentName);

        // encosta na criatura, como quem sobe na plataforma para atacar
        kaida.transform.position = boss.transform.position + new Vector3(-1.8f, -0.6f, 0f);
        kaida.SetFacing(1);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        var colisorDoChefe = boss.GetComponent<Collider2D>();
        Assert.IsNotNull(colisorDoChefe, "o Guardião está sem colisor");

        var area = new Vector2(kaida.attackRadius * 2f, kaida.attackRadius * 2.6f);
        var atingidos = Physics2D.OverlapBoxAll(
            kaida.attackPoint.position, area, 0f, kaida.enemyLayer);

        Assert.Greater(atingidos.Length, 0,
            $"o golpe não alcança o chefe. Kaida em {kaida.transform.position}, " +
            $"ponto de ataque em {kaida.attackPoint.position}, área {area}, " +
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
    public IEnumerator Chefe_DesceQuandoAOndaDeEcosEhLimpa()
    {
        // A fase 2 é a única em que ele se afasta de propósito. Se ele não
        // voltar depois que os ecos caem, a fase não tem como terminar: o
        // ataque da Kaida é corpo a corpo e o chefe fica pairando fora de
        // alcance para sempre.
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        var kaida = Object.FindObjectOfType<PlayerController>();
        Assert.IsNotNull(boss);

        yield return new WaitForSeconds(BossIntroState.Duracao + 0.5f);
        boss.AvancarFase();
        Assert.AreEqual("fase2", boss.Machine.CurrentName, "não entrou na fase 2");

        // deixa a onda entrar em campo e derruba todos os ecos de uma vez
        yield return new WaitForSeconds(1.5f);
        var ecos = Object.FindObjectsOfType<EnemyController>();
        Assert.Greater(ecos.Length, 0, "a fase 2 não chamou nenhum eco");
        foreach (var e in ecos) Object.Destroy(e.gameObject);
        yield return null;

        float melhorDistancia = float.MaxValue;
        for (float t = 0f; t < 5f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            melhorDistancia = Mathf.Min(melhorDistancia,
                Mathf.Abs(boss.transform.position.y - kaida.transform.position.y));
        }

        Assert.Less(melhorDistancia, 3.5f,
            $"com a onda limpa o chefe ficou a {melhorDistancia:F1} unidades de " +
            "altura do jogador: a fase 2 não abre janela de dano.");
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
