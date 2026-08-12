using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Dificuldade, pausa, morte e vitória - as partes que ficam em volta do
/// jogo em si, mas sem as quais ele não é entregável.
/// </summary>
public class MenusEDificuldadeTests
{
    Dificuldade dificuldadeOriginal;

    [SetUp]
    public void Antes()
    {
        // Ambiente limpo: tempo normal, camadas configuradas e sem restos
        // de cena de outro teste. Sem isso, um teste que congela o tempo ou
        // que abre uma região de verdade derruba os seguintes com erros que
        // não têm nada a ver com a causa.
        CenarioDeTeste.PrepararAmbiente();

        dificuldadeOriginal = GameSettings.Atual;
        Physics2D.IgnoreLayerCollision(CenarioDeTeste.LayerPlayer, CenarioDeTeste.LayerEnemy, true);
        CenarioDeTeste.CriarSistemas();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(60f, 2f));
    }

    [TearDown]
    public void Depois()
    {
        GameSettings.Atual = dificuldadeOriginal;
        Time.timeScale = 1f;
        CenarioDeTeste.Limpar();
    }

    // -------------------------------------------------------- dificuldade
    [Test]
    public void Dificuldade_Persiste()
    {
        GameSettings.Atual = Dificuldade.Dificil;
        Assert.AreEqual(Dificuldade.Dificil, GameSettings.Atual);

        GameSettings.Atual = Dificuldade.Facil;
        Assert.AreEqual(Dificuldade.Facil, GameSettings.Atual);
    }

    [Test]
    public void Dificuldade_TemNomeEDescricaoEmPortugues()
    {
        foreach (Dificuldade d in System.Enum.GetValues(typeof(Dificuldade)))
        {
            Assert.IsNotEmpty(GameSettings.Nome(d));
            Assert.IsNotEmpty(GameSettings.Descricao(d));
        }
        Assert.AreEqual("Fácil", GameSettings.Nome(Dificuldade.Facil), "com acento");
        Assert.AreEqual("Difícil", GameSettings.Nome(Dificuldade.Dificil), "com acento");
    }

    [Test]
    public void Dificuldade_MudaVidaEInvulnerabilidade()
    {
        var facil = CenarioDeTeste.StatsPadrao();
        GameSettings.Atual = Dificuldade.Facil;
        GameSettings.Aplicar(facil);

        var dificil = CenarioDeTeste.StatsPadrao();
        GameSettings.Atual = Dificuldade.Dificil;
        GameSettings.Aplicar(dificil);

        Assert.Greater(facil.maxHealth, dificil.maxHealth,
            "no fácil a Kaida aguenta mais golpes");
        Assert.Greater(facil.invulnTime, dificil.invulnTime,
            "no fácil a janela de recuperação é mais longa");
    }

    [Test]
    public void Dificuldade_MudaVelocidadeEVisaoDosInimigos()
    {
        GameSettings.Atual = Dificuldade.Facil;
        float vFacil = GameSettings.VelocidadeDosInimigos();
        float visaoFacil = GameSettings.AlcanceDeVisao();

        GameSettings.Atual = Dificuldade.Dificil;
        Assert.Greater(GameSettings.VelocidadeDosInimigos(), vFacil);
        Assert.Greater(GameSettings.AlcanceDeVisao(), visaoFacil);
    }

    [UnityTest]
    public IEnumerator Dificuldade_ChegaNaKaidaAoNascer()
    {
        GameSettings.Atual = Dificuldade.Dificil;
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;

        Assert.AreEqual(3, kaida.stats.maxHealth, "no difícil a vida máxima é 3");
        Assert.AreEqual(3, kaida.health, "e ela nasce com a vida cheia dessa dificuldade");
    }

    [UnityTest]
    public IEnumerator Stats_SaoClonados_ENaoAlteramOAssetOriginal()
    {
        // O PlayerStats é um asset em disco compartilhado. Se a dificuldade ou
        // um Nódulo de Vida escrevessem nele, a alteração ficaria gravada no
        // projeto e vazaria para a próxima partida.
        var original = CenarioDeTeste.StatsPadrao();
        original.maxHealth = 5;

        GameSettings.Atual = Dificuldade.Facil;
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f), original);
        yield return null;

        Assert.AreEqual(7, kaida.stats.maxHealth, "a cópia recebeu a dificuldade fácil");
        Assert.AreEqual(5, original.maxHealth, "o asset original não pode ter sido tocado");
        Assert.AreNotSame(original, kaida.stats, "a Kaida precisa usar uma cópia");
    }

    [UnityTest]
    public IEnumerator NoduloDeVida_NaoContaminaOAssetOriginal()
    {
        var original = CenarioDeTeste.StatsPadrao();
        GameSettings.Atual = Dificuldade.Normal;
        int antes = original.maxHealth;

        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f), original);
        yield return null;
        for (float t = 0f; t < 0.6f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        var go = new GameObject("Nodulo");
        go.transform.position = kaida.transform.position + Vector3.up;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.2f;
        go.AddComponent<HealthNode>().nodeId = "node_teste_isolamento";

        for (float t = 0f; t < 0.6f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        Assert.AreEqual(antes, original.maxHealth,
            "o nódulo aumentou a vida no asset em disco em vez de na partida");
    }

    // -------------------------------------------------------------- pausa
    [UnityTest]
    public IEnumerator Pausa_CongelaEDescongelaOTempo()
    {
        var go = new GameObject("Pausa");
        var pausa = go.AddComponent<PauseMenu>();
        yield return null;

        Assert.AreEqual(1f, Time.timeScale, "começa rodando");

        pausa.Pausar();
        Assert.IsTrue(PauseMenu.Pausado);
        Assert.AreEqual(0f, Time.timeScale, "pausado o tempo para");

        pausa.Retomar();
        Assert.IsFalse(PauseMenu.Pausado);
        Assert.AreEqual(1f, Time.timeScale, "ao retomar o tempo volta");
    }

    [UnityTest]
    public IEnumerator Pausa_NaoDeixaOTempoCongeladoAoSairDaCena()
    {
        var go = new GameObject("Pausa");
        var pausa = go.AddComponent<PauseMenu>();
        yield return null;

        pausa.Pausar();
        Assert.AreEqual(0f, Time.timeScale);

        Object.DestroyImmediate(go);
        yield return null;

        Assert.AreEqual(1f, Time.timeScale,
            "trocar de cena pausado deixaria o jogo inteiro congelado");
    }

    // ------------------------------------------------------------- telas
    [UnityTest]
    public IEnumerator Menus_SeAdaptamAoTamanhoDaTela()
    {
        // Com matchWidthOrHeight em 0 (o padrão), o canvas só acompanha a
        // largura: num ultrawide a interface cresce até estourar em cima e
        // embaixo. Casando pela altura (1) ela mantém o tamanho em qualquer
        // proporção e só sobra espaço nas laterais.
        var alvos = new[]
        {
            new GameObject("Pausa").AddComponent<PauseMenu>().gameObject,
            new GameObject("Morte").AddComponent<GameOverUI>().gameObject,
        };
        yield return null;

        foreach (var go in alvos)
        {
            var escala = go.GetComponent<UnityEngine.UI.CanvasScaler>();
            Assert.IsNotNull(escala, $"{go.name} está sem CanvasScaler");
            Assert.AreEqual(UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize, escala.uiScaleMode,
                $"{go.name} não acompanha o tamanho da tela");
            Assert.AreEqual(1f, escala.matchWidthOrHeight, 0.001f,
                $"{go.name} cortaria as opções em telas fora de 16:9");
        }
    }

    [UnityTest]
    public IEnumerator Camera_NaoMostraForaDoMapa_EmQualquerProporcao()
    {
        // 4:3 de projetor, 16:9 de notebook, 21:9 de monitor ultrawide.
        // Com margem fixa, o ultrawide passaria da borda e mostraria o vazio.
        var camGO = new GameObject("Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;

        var seguidor = camGO.AddComponent<CameraFollow2D>();
        seguidor.useBounds = true;
        seguidor.limiteMundoMin = new Vector2(0f, 0f);
        seguidor.limiteMundoMax = new Vector2(64f, 20f);
        seguidor.smoothTime = 0f;
        seguidor.offset = new Vector3(0f, 0f, -10f);

        // jogador colado na borda esquerda do mapa
        var alvo = new GameObject("Alvo");
        alvo.transform.position = new Vector3(1f, 1f, 0f);
        seguidor.target = alvo.transform;

        foreach (float proporcao in new[] { 4f / 3f, 16f / 9f, 21f / 9f })
        {
            cam.aspect = proporcao;
            yield return null;
            yield return null;

            float meiaLargura = cam.orthographicSize * proporcao;
            float bordaEsquerda = cam.transform.position.x - meiaLargura;

            // meia unidade de tolerância: a câmera chega na posição por
            // SmoothDamp, então nunca cravа o alvo exato num único frame
            Assert.GreaterOrEqual(bordaEsquerda, -0.5f,
                $"em {proporcao:0.00}:1 a câmera mostra {-bordaEsquerda:F1} unidades " +
                "para fora do mapa, do lado esquerdo");
        }
    }

    [UnityTest]
    public IEnumerator MenusComBotoes_TemEventSystem()
    {
        var go = new GameObject("Pausa");
        go.AddComponent<PauseMenu>();
        yield return null;

        Assert.IsNotNull(Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(),
            "sem EventSystem nenhum botão responde a clique");
        Assert.IsNotNull(go.GetComponent<UnityEngine.UI.GraphicRaycaster>(),
            "sem GraphicRaycaster o clique não chega no botão");
    }

    // ---------------------------------------------------------- game over
    [UnityTest]
    public IEnumerator TelaDeMorte_AssumeOControleDoRespawn()
    {
        var go = new GameObject("TelaDeMorte");
        go.AddComponent<GameOverUI>();
        yield return null;

        Assert.IsFalse(GameManager.Instance.respawnAutomatico,
            "com tela de morte na cena quem decide voltar é o jogador");
    }

    [UnityTest]
    public IEnumerator SemTelaDeMorte_ORespawnContinuaAutomatico()
    {
        yield return null;
        Assert.IsTrue(GameManager.Instance.respawnAutomatico,
            "sem a tela, o respawn automático evita travar a partida");
    }

    [UnityTest]
    public IEnumerator MorteDoJogador_DisparaOEventoUmaVez()
    {
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        for (float t = 0f; t < 0.6f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

        int avisos = 0;
        GameManager.Instance.PlayerMorreu += () => avisos++;

        kaida.TakeDamage(kaida.stats.maxHealth, Vector2.zero);
        yield return null;

        Assert.AreEqual(1, avisos, "a tela de morte precisa ser avisada exatamente uma vez");
    }
}
