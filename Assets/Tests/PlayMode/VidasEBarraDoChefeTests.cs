using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Duas coisas que o jogador vê e que a suíte não olhava: a barra do chefe
/// encolher de verdade e o limite de tentativas por partida.
/// </summary>
public class VidasEBarraDoChefeTests
{
    [SetUp]
    public void Antes()
    {
        CenarioDeTeste.PrepararAmbiente();
        CenarioDeTeste.CriarSistemas();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));
    }

    [TearDown]
    public void Depois() => CenarioDeTeste.Limpar();

    // ------------------------------------------------------------ barra do chefe
    [UnityTest]
    public IEnumerator BarraDoChefe_EncolheAoLevarDano()
    {
        // Antes a barra era uma Image com type Filled e nenhum sprite. Nessa
        // combinação o fillAmount não faz nada: dava para bater no chefe a
        // luta inteira e a barra continuava cheia.
        var boss = CriarChefe(new Vector2(20f, 5f));
        var hud = new GameObject("BossHUD").AddComponent<BossHealthUI>();
        hud.boss = boss;
        yield return null;
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.4f);

        var barra = AcharBarra(hud);
        Assert.IsNotNull(barra, "a barra do chefe não foi montada");

        float cheia = barra.anchorMax.x;
        Assert.Greater(cheia, 0.99f, "a barra deveria começar cheia");

        boss.TakeDamage(boss.maxHealth / 2, Vector2.zero);
        yield return null;

        Assert.Less(barra.anchorMax.x, cheia - 0.2f,
            $"o chefe perdeu metade da vida e a barra foi de {cheia:F2} para " +
            $"{barra.anchorMax.x:F2}: quem está jogando não vê que o golpe entrou.");
    }

    [UnityTest]
    public IEnumerator BarraDoChefe_ZeraQuandoAVidaAcaba()
    {
        var boss = CriarChefe(new Vector2(20f, 5f));
        var hud = new GameObject("BossHUD").AddComponent<BossHealthUI>();
        hud.boss = boss;
        yield return null;
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.4f);

        var barra = AcharBarra(hud);
        boss.TakeDamage(boss.maxHealth, Vector2.zero);
        yield return null;

        Assert.AreEqual(0f, barra.anchorMax.x, 0.01f, "a barra não chegou a zero");
        Assert.IsTrue(boss.Derrotado);
    }

    static RectTransform AcharBarra(BossHealthUI hud)
    {
        foreach (var rt in hud.GetComponentsInChildren<RectTransform>(true))
            if (rt.name == "Preenchimento") return rt;
        return null;
    }

    // ------------------------------------------------------------------ vidas
    [UnityTest]
    public IEnumerator TresQuedas_ZeramAsTentativas()
    {
        var gm = GameManager.Instance;
        Assert.IsNotNull(gm, "sem GameManager não há como contar tentativas");
        gm.ReiniciarVidas();

        Assert.AreEqual(GameManager.VidasPorPartida, gm.VidasRestantes);
        Assert.IsFalse(gm.AcabaramAsVidas);

        for (int i = 1; i < GameManager.VidasPorPartida; i++)
        {
            gm.ConsumirVida();
            Assert.IsFalse(gm.AcabaramAsVidas,
                $"na queda {i} ainda deveria sobrar tentativa");
        }

        gm.ConsumirVida();
        Assert.IsTrue(gm.AcabaramAsVidas, "a terceira queda encerra a partida");
        Assert.AreEqual(0, gm.VidasRestantes);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MorrerConsomeUmaTentativa_ESemTentativaNaoRenasce()
    {
        var gm = GameManager.Instance;
        gm.ReiniciarVidas();

        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        // a Kaida já se registra sozinha ao acordar: registrar de novo aqui é
        // de propósito, para provar que não conta a morte duas vezes
        gm.RegisterPlayer(kaida);
        gm.SetCheckpoint(new Vector2(0f, 1f));
        yield return null;

        kaida.TakeDamage(999, Vector2.zero);
        yield return null;
        Assert.AreEqual(GameManager.VidasPorPartida - 1, gm.VidasRestantes,
            "morrer tem que gastar uma tentativa");

        // gasta as que sobraram e confere que o respawn automático para
        while (!gm.AcabaramAsVidas) gm.ConsumirVida();

        kaida.health = kaida.stats.maxHealth;
        kaida.Machine.ChangeState("idle");
        kaida.transform.position = new Vector2(30f, 1f);
        // sem isto a janela de invulnerabilidade da primeira morte engole o
        // segundo golpe, e a Kaida nem chega a morrer de novo
        kaida.CancelInvulnWindow();
        yield return new WaitForFixedUpdate();

        kaida.TakeDamage(999, Vector2.zero);
        Assert.AreEqual("dead", kaida.CurrentStateName, "a segunda morte não aconteceu");

        for (float t = 0f; t < 1.6f; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        Assert.AreEqual("dead", kaida.CurrentStateName,
            "sem tentativas o jogo não pode devolver o jogador sozinho: " +
            "quem decide o recomeço é a tela de morte");
    }

    [UnityTest]
    public IEnumerator TentativasVoltamAoCheio_AoComecarDeNovo()
    {
        var gm = GameManager.Instance;
        gm.ReiniciarVidas();
        while (!gm.AcabaramAsVidas) gm.ConsumirVida();

        gm.ReiniciarVidas();
        Assert.AreEqual(GameManager.VidasPorPartida, gm.VidasRestantes,
            "partida nova começa com as três tentativas");
        yield return null;
    }

    GuardianBoss CriarChefe(Vector2 pos)
    {
        var go = new GameObject("Guardiao") { layer = CenarioDeTeste.LayerEnemy };
        go.SetActive(false);
        go.transform.position = pos;
        go.AddComponent<SpriteRenderer>();
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>().radius = 0.8f;

        var boss = go.AddComponent<GuardianBoss>();
        boss.maxHealth = 10;
        boss.playerLayer = CenarioDeTeste.MaskPlayer;

        go.SetActive(true);
        return boss;
    }
}
