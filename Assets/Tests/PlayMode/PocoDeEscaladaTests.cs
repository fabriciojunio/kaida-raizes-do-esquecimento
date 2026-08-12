using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// A subida do poço da Caverna, na cena de verdade.
///
/// O validador de mapas garante que o poço existe e tem parede dos dois lados,
/// mas isso é geometria. O que faltava era conferir que a Kaida agarra nas
/// duas paredes e que o salto atravessa o vão - foi exatamente aí que ela
/// ficou presa no pé do poço enquanto o mapa passava em todos os testes.
/// </summary>
public class PocoDeEscaladaTests
{
    [TearDown]
    public void Depois()
    {
        Time.timeScale = 1f;
        CenarioDeTeste.Limpar();
    }

    /// <summary>
    /// Altura livre para medir o vão: acima do descanso do meio, senão o raio
    /// bate na plataforma de descanso e o poço parece ter uma unidade de largura.
    /// </summary>
    static float AlturaLivre(Rect area) => area.yMin + area.height * 0.75f;

    [UnityTest]
    public IEnumerator Kaida_AgarraNasDuasParedesDoPoco()
    {
        SceneManager.LoadScene("04_CavernaMusgosa");
        yield return null;
        yield return null;

        var kaida = Object.FindObjectOfType<PlayerController>();
        var poco = Object.FindObjectOfType<PocoDeEscalada>();
        Assert.IsNotNull(kaida, "sem Kaida na Caverna");
        Assert.IsNotNull(poco, "a Caverna está sem poço de escalada");

        if (SaveSystem.Instance != null) SaveSystem.Instance.UnlockAbility("wall_climb");

        float altura = AlturaLivre(poco.area);
        float esquerda = FaceDaParede(poco.area, altura, -1);
        float direita = FaceDaParede(poco.area, altura, +1);
        float meioDoVao = (esquerda + direita) * 0.5f;

        foreach (int lado in new[] { +1, -1 })
        {
            yield return Encostar(kaida, new Vector2(meioDoVao, altura), lado);

            Assert.AreEqual(lado, kaida.LadoDaParede(),
                $"a parede da {(lado > 0 ? "direita" : "esquerda")} do poço não foi " +
                $"detectada. Kaida em x={kaida.transform.position.x:F2}, paredes em " +
                $"{esquerda:F1} e {direita:F1}.");

            Assert.IsTrue(kaida.CanWallCling(),
                $"não dá para agarrar na parede da {(lado > 0 ? "direita" : "esquerda")}");
        }
    }

    [UnityTest]
    public IEnumerator OSaltoDeParede_AtravessaOVaoDoPoco()
    {
        SceneManager.LoadScene("04_CavernaMusgosa");
        yield return null;
        yield return null;

        var kaida = Object.FindObjectOfType<PlayerController>();
        var poco = Object.FindObjectOfType<PocoDeEscalada>();

        float altura = AlturaLivre(poco.area);
        float vao = FaceDaParede(poco.area, altura, +1) - FaceDaParede(poco.area, altura, -1);

        Assert.Greater(vao, 1.5f, "o poço não tem vão nenhum");

        // distância horizontal percorrida entre sair de uma parede e voltar à
        // mesma altura: o trecho travado sai na força do salto, o resto na
        // velocidade de corrida
        float subida = kaida.stats.JumpVelocity * kaida.stats.wallJumpPower;
        float tempoDeVoo = subida / kaida.stats.JumpGravity + subida / kaida.stats.FallGravity;
        float travado = Mathf.Min(kaida.stats.wallJumpLockTime, tempoDeVoo);
        float alcance = kaida.stats.wallJumpForceX * travado
                      + kaida.stats.runSpeed * (tempoDeVoo - travado);

        Assert.Greater(alcance, vao * 1.3f,
            $"o salto de parede cruza {alcance:F1} unidades e o vão do poço tem " +
            $"{vao:F1}. Sem folga, atravessar depende do frame exato e a subida " +
            "vira tentativa e erro.");
    }

    [UnityTest]
    public IEnumerator OTopoDoPoco_LevaParaOSantuario()
    {
        // Chegar em cima e não passar de região deixaria o poço sendo esforço
        // sem recompensa - e foi assim que ele apareceu jogando.
        SceneManager.LoadScene("04_CavernaMusgosa");
        yield return null;
        yield return null;

        var poco = Object.FindObjectOfType<PocoDeEscalada>();
        var passagem = System.Array.Find(Object.FindObjectsOfType<RoomTransition>(),
                                         t => t.targetScene == "05_SantuarioEsquecido");
        Assert.IsNotNull(passagem, "a Caverna não tem passagem para o Santuário");

        Assert.IsTrue(poco.area.Contains(passagem.transform.position),
            $"a passagem está em {passagem.transform.position} e o poço vai de " +
            $"{poco.area.min} a {poco.area.max}: quem sobe o poço não encosta nela");

        var kaida = Object.FindObjectOfType<PlayerController>();
        kaida.transform.position = passagem.transform.position;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual("05_SantuarioEsquecido", SceneManager.GetActiveScene().name,
            "encostar na passagem no alto do poço não trocou de região");
    }

    /// <summary>Onde está a parede daquele lado, partindo do centro do poço.</summary>
    static float FaceDaParede(Rect area, float altura, int lado)
    {
        var origem = new Vector2(area.center.x, altura);
        var hit = Physics2D.Raycast(origem, Vector2.right * lado, area.width,
                                    CenarioDeTeste.MaskGround);
        Assert.IsTrue(hit.collider != null,
            $"não há parede {(lado > 0 ? "à direita" : "à esquerda")} na altura {altura:F0}");
        return hit.point.x;
    }

    /// <summary>Empurra a Kaida contra a parede daquele lado e deixa assentar.</summary>
    static IEnumerator Encostar(PlayerController kaida, Vector2 partida, int lado)
    {
        kaida.transform.position = partida;
        kaida.SetFacing(lado);
        yield return new WaitForFixedUpdate();

        // sem gravidade durante a medição: aqui interessa o contato lateral
        float gravidade = kaida.rb.gravityScale;
        kaida.rb.gravityScale = 0f;

        for (int i = 0; i < 25; i++)
        {
            kaida.SetVelocity(lado * 6f, 0f);
            yield return new WaitForFixedUpdate();
        }
        kaida.SetVelocity(0f, 0f);
        yield return new WaitForFixedUpdate();

        kaida.rb.gravityScale = gravidade;
    }
}
