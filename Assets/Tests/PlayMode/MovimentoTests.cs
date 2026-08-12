using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Movimento e game feel com a física rodando de verdade.
/// </summary>
public class MovimentoTests
{
    PlayerController kaida;

    [SetUp]
    public void Antes()
    {
        // Ambiente limpo: tempo normal, camadas configuradas e sem restos
        // de cena de outro teste. Sem isso, um teste que congela o tempo ou
        // que abre uma região de verdade derruba os seguintes com erros que
        // não têm nada a ver com a causa.
        CenarioDeTeste.PrepararAmbiente();

        Physics2D.IgnoreLayerCollision(CenarioDeTeste.LayerPlayer, CenarioDeTeste.LayerEnemy, true);
        CenarioDeTeste.CriarSistemas();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(60f, 2f));
    }

    [TearDown]
    public void Depois() => CenarioDeTeste.Limpar();

    [UnityTest]
    public IEnumerator Kaida_CaiPelaGravidade_ePousaNoChao()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6f));
        yield return null;

        Assert.Greater(kaida.transform.position.y, 3f, "deveria começar no alto");

        // tempo de sobra para cair os 6 metros
        for (float t = 0f; t < 2f; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        Assert.IsTrue(kaida.IsGrounded(), "a Kaida deveria ter pousado");
        Assert.AreEqual(0f, kaida.transform.position.y, 0.4f, "deveria parar em cima do chão");
    }

    [UnityTest]
    public IEnumerator AoPousar_EntraEmIdle()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 5f));
        yield return null;

        for (float t = 0f; t < 2f; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        Assert.AreEqual("idle", kaida.CurrentStateName);
    }

    [UnityTest]
    public IEnumerator NaoAtravessaOChao_MesmoCaindoDeMuitoAlto()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 40f));
        yield return null;

        for (float t = 0f; t < 4f; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        Assert.Greater(kaida.transform.position.y, -2f,
            "caiu através do chão: detecção de colisão contínua não está funcionando");
    }

    [UnityTest]
    public IEnumerator Pulo_SobeAproximadamenteAAlturaConfigurada()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        float chao = kaida.transform.position.y;
        kaida.Machine.ChangeState("jump");

        float maisAlto = chao;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            maisAlto = Mathf.Max(maisAlto, kaida.transform.position.y);
        }

        float subiu = maisAlto - chao;
        // a integração discreta perde um pouco da altura teórica
        Assert.Greater(subiu, kaida.stats.jumpHeight * 0.75f,
            $"o pulo subiu só {subiu:F2} de {kaida.stats.jumpHeight} configurados");
        Assert.Less(subiu, kaida.stats.jumpHeight * 1.3f, "o pulo subiu bem mais que o configurado");
    }

    [UnityTest]
    public IEnumerator DepoisDoPulo_VoltaAoChao()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        kaida.Machine.ChangeState("jump");
        for (float t = 0f; t < 3f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        Assert.IsTrue(kaida.IsGrounded(), "o que sobe tem que descer");
    }

    [UnityTest]
    public IEnumerator Dash_MoveNaHorizontalEDaInvulnerabilidade()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        float xInicial = kaida.transform.position.x;
        kaida.SetFacing(1);
        kaida.Machine.ChangeState("dash");
        yield return new WaitForFixedUpdate();

        Assert.IsTrue(kaida.isInvulnerable, "o dash precisa dar i-frames: é a defesa contra os feixes do chefe");

        for (float t = 0f; t < kaida.stats.dashTime; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        float andou = kaida.transform.position.x - xInicial;
        Assert.Greater(andou, 1.5f, $"o dash só andou {andou:F2} unidades");
    }

    [UnityTest]
    public IEnumerator Dash_NaoAtravessaParede()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        CenarioDeTeste.CriarParede(new Vector2(4f, 2f), new Vector2(1f, 6f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        kaida.SetFacing(1);
        kaida.Machine.ChangeState("dash");
        for (float t = 0f; t < 0.5f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        Assert.Less(kaida.transform.position.x, 4f, "o dash atravessou a parede");
    }

    [UnityTest]
    public IEnumerator SemHabilidade_NaoTemPuloDuplo()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        SaveSystem.Instance.Data = new SaveData();   // ninguém desbloqueou nada
        kaida.RefreshAirAbilities();

        Assert.IsFalse(kaida.CanAirJump(), "pulo duplo não pode existir antes de ser encontrado no mapa");
    }

    [UnityTest]
    public IEnumerator ComHabilidade_GanhaUmPuloNoAr()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        SaveSystem.Instance.Data.unlockedAbilities.Add("double_jump");
        kaida.RefreshAirAbilities();

        Assert.IsTrue(kaida.CanAirJump(), "com a habilidade deveria haver um pulo no ar");

        kaida.ConsumeAirJump();
        Assert.IsFalse(kaida.CanAirJump(), "só existe um pulo extra: o segundo tem que acabar");
    }

    [UnityTest]
    public IEnumerator PuloDuplo_SobeMaisQueOPuloSimples()
    {
        // pulo simples
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        float chao = kaida.transform.position.y;
        kaida.Machine.ChangeState("jump");
        float altoSimples = chao;
        for (float t = 0f; t < 1.2f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            altoSimples = Mathf.Max(altoSimples, kaida.transform.position.y);
        }

        // com o segundo pulo disparado no meio da subida
        Object.DestroyImmediate(kaida.gameObject);
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        SaveSystem.Instance.Data.unlockedAbilities.Add("double_jump");
        kaida.RefreshAirAbilities();
        kaida.Machine.ChangeState("jump");

        float altoDuplo = kaida.transform.position.y;
        bool disparou = false;
        for (float t = 0f; t < 1.6f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            // dispara o segundo pulo quando a subida começa a acabar
            if (!disparou && kaida.rb.velocity.y < 1f && !kaida.IsGrounded())
            {
                kaida.ConsumeAirJump();
                kaida.Machine.ChangeState("fall");
                kaida.Machine.ChangeState("jump");
                disparou = true;
            }
            altoDuplo = Mathf.Max(altoDuplo, kaida.transform.position.y);
        }

        Assert.Greater(altoDuplo, altoSimples + 1f,
            $"o pulo duplo ({altoDuplo:F2}) deveria passar bem do simples ({altoSimples:F2})");
    }

    [UnityTest]
    public IEnumerator SemHabilidade_NaoAgarraNaParede()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6f));
        CenarioDeTeste.CriarParede(new Vector2(1.2f, 6f), new Vector2(1f, 10f));
        yield return null;

        SaveSystem.Instance.Data = new SaveData();
        kaida.SetFacing(1);
        yield return new WaitForFixedUpdate();

        Assert.IsFalse(kaida.CanWallCling(),
            "sem a Escalada de Parede não pode grudar: quebraria a progressão do metroidvania");
    }

    [UnityTest]
    public IEnumerator ComHabilidade_DeslizaNaParedeEmVezDeCair()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 8f));
        CenarioDeTeste.CriarParede(new Vector2(1.1f, 8f), new Vector2(1f, 12f));
        yield return null;

        SaveSystem.Instance.Data.unlockedAbilities.Add("wall_climb");
        kaida.SetFacing(1);

        // O estado é exercitado direto porque manter-se agarrado depende de
        // segurar a direção, e não há como injetar Input em teste automatizado.
        // O que importa medir aqui é a velocidade de descida.
        var agarrado = new PlayerWallClingState(kaida);
        agarrado.SetMachine(kaida.Machine);
        agarrado.Enter();
        agarrado.PhysicsUpdate();

        // medido no mesmo frame: esperar o próximo FixedUpdate deixaria o
        // estado de verdade da máquina (queda) sobrescrever a velocidade
        Assert.AreEqual(-kaida.stats.wallSlideSpeed, kaida.rb.velocity.y, 0.01f,
            "agarrada na parede a descida é constante, não em queda livre");
        Assert.Less(kaida.stats.wallSlideSpeed, kaida.stats.maxFallSpeed,
            "deslizar tem que ser mais lento que cair");
    }

    [UnityTest]
    public IEnumerator AgarrarNaParede_RecarregaDashEPuloDuplo()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 8f));
        CenarioDeTeste.CriarParede(new Vector2(1.1f, 8f), new Vector2(1f, 12f));
        yield return null;

        SaveSystem.Instance.Data.unlockedAbilities.Add("wall_climb");
        SaveSystem.Instance.Data.unlockedAbilities.Add("double_jump");
        kaida.SetFacing(1);

        kaida.airDashesLeft = 0;
        kaida.airJumpsLeft = 0;

        var agarrado = new PlayerWallClingState(kaida);
        agarrado.SetMachine(kaida.Machine);
        agarrado.Enter();
        yield return null;

        Assert.Greater(kaida.airDashesLeft, 0, "agarrar na parede devolve o dash");
        Assert.IsTrue(kaida.CanAirJump(), "agarrar na parede devolve o pulo duplo");
    }
}
