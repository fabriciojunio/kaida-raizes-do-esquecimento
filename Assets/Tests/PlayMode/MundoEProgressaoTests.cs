using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Coletáveis, perigos, checkpoints e o chefe - a progressão do metroidvania.
/// </summary>
public class MundoEProgressaoTests
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
        SaveSystem.Instance.Data = new SaveData();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));
    }

    [TearDown]
    public void Depois()
    {
        if (SaveSystem.Instance != null) SaveSystem.Instance.DeleteSave();
        CenarioDeTeste.Limpar();
    }

    IEnumerator Assentar(float s = 1f)
    {
        for (float t = 0f; t < s; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
    }

    // ------------------------------------------------------------- coletáveis
    [UnityTest]
    public IEnumerator PegarHabilidade_DesbloqueiaEUsaNaHora()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        var go = new GameObject("Habilidade");
        go.transform.position = kaida.transform.position + Vector3.up * 1f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.2f;
        var pickup = go.AddComponent<PickupAbility>();
        pickup.abilityId = "double_jump";

        Assert.IsFalse(SaveSystem.Instance.HasAbility("double_jump"));

        yield return Assentar(0.5f);

        Assert.IsTrue(SaveSystem.Instance.HasAbility("double_jump"),
            "encostar no coletável tem que desbloquear a habilidade");
        Assert.IsTrue(kaida.CanAirJump(),
            "a habilidade precisa valer imediatamente, sem ter que tocar o chão antes");
        Assert.IsFalse(go.activeSelf, "o coletável some depois de pego");
    }

    [UnityTest]
    public IEnumerator HabilidadeJaPega_NaoReaparece()
    {
        SaveSystem.Instance.Data.unlockedAbilities.Add("wall_climb");

        var go = new GameObject("Habilidade");
        go.transform.position = new Vector3(20f, 5f, 0f);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var pickup = go.AddComponent<PickupAbility>();
        pickup.abilityId = "wall_climb";

        yield return null;   // deixa o Start rodar

        Assert.IsFalse(go.activeSelf,
            "voltar numa sala antiga não pode mostrar de novo uma habilidade já pega");
    }

    [UnityTest]
    public IEnumerator NoduloDeVida_AumentaOMaximoPermanentemente()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        int maxAntes = kaida.stats.maxHealth;

        var go = new GameObject("Nodulo");
        go.transform.position = kaida.transform.position + Vector3.up * 1f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.2f;
        go.AddComponent<HealthNode>().nodeId = "node_teste";

        yield return Assentar(0.5f);

        Assert.AreEqual(maxAntes + 1, kaida.stats.maxHealth, "o nódulo dá +1 de vida máxima");
        Assert.IsTrue(SaveSystem.Instance.IsCollected("node_teste"));
    }

    [UnityTest]
    public IEnumerator FragmentoDeLore_MarcaComoLidoENaoVolta()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        var go = new GameObject("Fragmento");
        go.transform.position = kaida.transform.position + Vector3.up * 1f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.2f;
        var frag = go.AddComponent<LoreFragment>();
        frag.fragmentId = "frag_teste";
        frag.texto = "uma memória qualquer";

        yield return Assentar(0.5f);

        Assert.IsTrue(SaveSystem.Instance.IsCollected("frag_teste"));
        Assert.IsFalse(go.activeSelf);
    }

    // ---------------------------------------------------------------- perigos
    [UnityTest]
    public IEnumerator Espinhos_CausamDanoEDevolvemAoCheckpoint()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(10f, 1f));
        yield return null;
        yield return Assentar();

        GameManager.Instance.SetCheckpoint(new Vector2(0f, 1f), "teste");
        int vidaAntes = kaida.health;

        var go = new GameObject("Espinhos");
        go.transform.position = kaida.transform.position + Vector3.up * 0.5f;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 2f);
        var h = go.AddComponent<Hazard>();
        h.damage = 1;
        h.returnToCheckpoint = true;

        yield return Assentar(0.5f);

        Assert.Less(kaida.health, vidaAntes, "os espinhos precisam machucar");
        Assert.AreEqual(0f, kaida.transform.position.x, 1.5f,
            "cair no perigo devolve ao checkpoint em vez de deixar preso dentro dele");
    }

    // ------------------------------------------------------------- checkpoint
    [UnityTest]
    public IEnumerator Checkpoint_GravaAPosicaoAoEncostar()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(7f, 1f));
        yield return null;
        yield return Assentar();

        var go = new GameObject("Marco");
        go.transform.position = kaida.transform.position;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 3f);
        go.AddComponent<Checkpoint>();

        yield return Assentar(0.5f);

        Assert.AreEqual(go.transform.position.x, GameManager.Instance.CurrentCheckpoint.x, 0.5f,
            "encostar no marco deveria gravar o ponto de retorno");
    }

    // ------------------------------------------------------------------ chefe
    [UnityTest]
    public IEnumerator Chefe_ComecaNaFase1EFicaIntocavelNaIntro()
    {
        var boss = CriarChefe(new Vector2(20f, 5f));
        yield return null;

        Assert.AreEqual(1, boss.FaseAtual);

        int vidaAntes = boss.Health;
        boss.TakeDamage(5, Vector2.zero);      // ainda na abertura
        Assert.AreEqual(vidaAntes, boss.Health, "durante a intro o chefe não recebe dano");
    }

    [UnityTest]
    public IEnumerator Chefe_AvancaDeFaseAoZerarAVida()
    {
        var boss = CriarChefe(new Vector2(20f, 5f));
        yield return null;
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.6f);   // passa a intro

        Assert.AreEqual("fase1", boss.Machine.CurrentName);

        boss.TakeDamage(boss.healthFase1, Vector2.zero);
        yield return null;
        Assert.AreEqual("transicao", boss.Machine.CurrentName,
            "zerar a vida da fase 1 leva para a transição, não mata");

        yield return new WaitForSeconds(BossIntroState.Duracao + 0.6f);
        Assert.AreEqual(2, boss.FaseAtual, "deveria ter virado a fase 2");
        Assert.AreEqual(boss.healthFase2, boss.Health, "a fase nova começa com a vida dela");
    }

    [UnityTest]
    public IEnumerator Chefe_SoMorreDepoisDasTresFases()
    {
        var boss = CriarChefe(new Vector2(20f, 5f));
        yield return null;
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.6f);

        boss.TakeDamage(boss.healthFase1, Vector2.zero);
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.6f);
        Assert.IsFalse(boss.Derrotado, "não pode morrer na fase 1");

        boss.TakeDamage(boss.healthFase2, Vector2.zero);
        yield return new WaitForSeconds(BossIntroState.Duracao + 0.6f);
        Assert.IsFalse(boss.Derrotado, "não pode morrer na fase 2");
        Assert.AreEqual(3, boss.FaseAtual);

        boss.TakeDamage(boss.healthFase3, Vector2.zero);
        yield return null;
        Assert.IsTrue(boss.Derrotado, "com as três fases zeradas ele cai");
    }

    [UnityTest]
    public IEnumerator FeixeDoChefe_MachucaAKaidaESome()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        int vidaAntes = kaida.health;

        var go = new GameObject("Feixe");
        go.transform.position = kaida.transform.position + Vector3.up * 1.4f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;
        var beam = go.AddComponent<LumenBeam>();
        beam.dano = 1;
        beam.paredes = CenarioDeTeste.MaskGround;
        beam.Lancar(Vector2.right, 0.1f);

        yield return Assentar(0.4f);

        Assert.Less(kaida.health, vidaAntes, "o feixe deveria ter acertado");
        Assert.IsTrue(go == null, "o feixe some depois de acertar");
    }

    [UnityTest]
    public IEnumerator FeixeDoChefe_SomeSozinhoDepoisDoTempo()
    {
        var go = new GameObject("Feixe");
        go.transform.position = new Vector3(40f, 20f, 0f);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var beam = go.AddComponent<LumenBeam>();
        beam.tempoDeVida = 0.3f;
        beam.paredes = CenarioDeTeste.MaskGround;
        beam.Lancar(Vector2.up, 1f);

        yield return new WaitForSeconds(0.6f);

        Assert.IsTrue(go == null, "projétil sem prazo de validade vaza memória a partida inteira");
    }

    /// <summary>
    /// Nasce desativado: o Awake do chefe copia healthFase1 para a vida atual,
    /// então baixar a vida das fases depois de ele acordar não teria efeito e
    /// os testes nunca chegariam na fase 2.
    /// </summary>
    GuardianBoss CriarChefe(Vector2 pos)
    {
        var go = new GameObject("Guardiao") { layer = CenarioDeTeste.LayerEnemy };
        go.SetActive(false);
        go.transform.position = pos;
        go.AddComponent<SpriteRenderer>();
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.8f;

        var boss = go.AddComponent<GuardianBoss>();
        boss.healthFase1 = 4;
        boss.healthFase2 = 4;
        boss.healthFase3 = 4;
        boss.playerLayer = CenarioDeTeste.MaskPlayer;
        boss.ecoPrefabs = new GameObject[0];
        boss.pontosDeInvocacao = new Transform[0];

        go.SetActive(true);
        return boss;
    }
}
