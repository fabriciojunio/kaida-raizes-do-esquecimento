using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Combate, dano, morte e respawn — com colisores e física reais.
/// </summary>
public class CombateTests
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
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));
    }

    [TearDown]
    public void Depois() => CenarioDeTeste.Limpar();

    IEnumerator Assentar(float segundos = 1f)
    {
        for (float t = 0f; t < segundos; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();
    }

    // ------------------------------------------------------------ dano recebido
    [UnityTest]
    public IEnumerator LevarDano_TiraVidaEDaKnockback()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        int vidaAntes = kaida.health;
        kaida.TakeDamage(1, new Vector2(2f, 1f));   // golpe vindo da direita
        yield return new WaitForFixedUpdate();

        Assert.AreEqual(vidaAntes - 1, kaida.health);
        Assert.AreEqual("hurt", kaida.CurrentStateName);
        Assert.Less(kaida.rb.velocity.x, 0f, "o empurrão tem que ser para longe de quem bateu");
    }

    [UnityTest]
    public IEnumerator Invulneravel_IgnoraODanoSeguinte()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        kaida.isInvulnerable = true;
        int vidaAntes = kaida.health;
        kaida.TakeDamage(1, Vector2.zero);
        yield return null;

        Assert.AreEqual(vidaAntes, kaida.health, "invulnerável não perde vida");
    }

    [UnityTest]
    public IEnumerator DepoisDoDano_FicaInvulneravelPorUmTempoEDepoisVolta()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        yield return null;
        yield return Assentar();

        kaida.TakeDamage(1, new Vector2(3f, 1f));

        // o estado hurt dura ~0,25s e só então abre a janela de invulnerabilidade
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(kaida.isInvulnerable, "logo após apanhar deveria estar invulnerável");

        yield return new WaitForSeconds(kaida.stats.invulnTime + 0.3f);
        Assert.IsFalse(kaida.isInvulnerable, "a invulnerabilidade não pode durar para sempre");
    }

    [UnityTest]
    public IEnumerator Morrer_EntraEmDead_ERespawnDevolveVidaCheia()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(5f, 1f));
        yield return null;
        yield return Assentar();

        // o checkpoint fica na superfície do chão (topo em y=0), senão a Kaida
        // reaparece no ar e o estado correto passa a ser "fall", não "idle"
        GameManager.Instance.SetCheckpoint(new Vector2(0f, 0f), "teste");

        int vidaRecebida = -1;
        kaida.HealthChanged += (atual, max) => vidaRecebida = atual;

        kaida.TakeDamage(kaida.stats.maxHealth, Vector2.zero);
        yield return null;

        Assert.AreEqual(0, kaida.health);
        Assert.AreEqual("dead", kaida.CurrentStateName);

        GameManager.Instance.RespawnPlayer();
        yield return Assentar(0.4f);

        Assert.AreEqual(kaida.stats.maxHealth, kaida.health, "respawn devolve a vida cheia");
        Assert.AreEqual("idle", kaida.CurrentStateName);
        Assert.AreEqual(0f, kaida.transform.position.x, 0.1f, "deveria voltar ao checkpoint");
        Assert.AreEqual(kaida.stats.maxHealth, vidaRecebida,
            "a HUD precisa ser avisada no respawn, senão fica mostrando vida zerada");
        Assert.IsFalse(kaida.isInvulnerable, "o respawn não pode deixar invulnerabilidade presa");
    }

    [UnityTest]
    public IEnumerator Respawn_ZeraAVelocidade()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(5f, 6f));
        yield return null;
        yield return Assentar(0.5f);   // ganhando velocidade de queda

        GameManager.Instance.SetCheckpoint(new Vector2(0f, 1f), "teste");
        kaida.TakeDamage(kaida.stats.maxHealth, Vector2.zero);
        yield return null;
        GameManager.Instance.RespawnPlayer();
        yield return null;

        Assert.AreEqual(0f, kaida.rb.velocity.magnitude, 0.01f,
            "reaparecer com velocidade acumulada joga a Kaida para fora do checkpoint");
    }

    // ------------------------------------------------------------ dano causado
    [UnityTest]
    public IEnumerator Ataque_FereOInimigoNaFrente()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        // parado de propósito: com a patrulha ligada ele sairia do alcance
        // durante a espera e o teste mediria a distância, não o golpe
        var alvo = CenarioDeTeste.CriarInimigo<EnemyController>(new Vector2(1.3f, 1f),
            configurar: e => { e.maxHealth = 5; e.moveSpeed = 0f; e.detectRange = 0f; });
        yield return null;
        yield return Assentar();

        kaida.SetFacing(1);

        // Colocado em cima do ponto de ataque, e só depois que os dois
        // assentaram: o inimigo tem colisor mais alto que a Kaida, então
        // assenta mais baixo e escapa por baixo do arco do golpe.
        alvo.transform.position = kaida.attackPoint.position;
        alvo.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // confere primeiro que o alvo está mesmo dentro do arco: se falhar
        // aqui, o problema é posicionamento; se passar e o dano não sair, o
        // problema é no DoAttackHit
        var dentroDoArco = Physics2D.OverlapCircleAll(
            kaida.attackPoint.position, kaida.attackRadius, kaida.enemyLayer);
        Assert.Greater(dentroDoArco.Length, 0,
            $"nada dentro do arco: alvo em {alvo.transform.position}, " +
            $"ponto de ataque em {kaida.attackPoint.position}, " +
            $"raio {kaida.attackRadius}, máscara {kaida.enemyLayer.value}, " +
            $"layer do alvo {alvo.gameObject.layer}");

        var comoAlvo = dentroDoArco[0].GetComponentInParent<IDamageable>();
        Assert.IsNotNull(comoAlvo, "o inimigo encontrado não responde como IDamageable");

        int vidaAntes = alvo.Health;
        kaida.DoAttackHit();
        yield return null;

        Assert.IsFalse(alvo == null,
            $"um único golpe de {kaida.stats.attackDamage} matou um inimigo com " +
            $"{vidaAntes} de vida: o dano está sendo aplicado mais de uma vez");

        Assert.Less(alvo.Health, vidaAntes, "o inimigo à frente deveria ter levado o golpe");
        Assert.AreEqual(vidaAntes - kaida.stats.attackDamage, alvo.Health,
            "o golpe deve tirar exatamente o dano de ataque, nem mais");
    }

    [UnityTest]
    public IEnumerator Ataque_NaoAlcancaQuemEstaLonge()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        var longe = CenarioDeTeste.CriarInimigo<EnemyController>(new Vector2(12f, 1f));
        yield return null;
        yield return Assentar();

        int vidaAntes = longe.Health;
        kaida.SetFacing(1);
        kaida.DoAttackHit();
        yield return null;

        Assert.AreEqual(vidaAntes, longe.Health, "o ataque não pode acertar do outro lado da tela");
    }

    [UnityTest]
    public IEnumerator Inimigo_MorreAoZerarVida()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        var alvo = CenarioDeTeste.CriarInimigo<EnemyController>(new Vector2(1.3f, 1f),
            configurar: e => { e.maxHealth = 1; e.moveSpeed = 0f; e.detectRange = 0f; });
        yield return null;
        yield return Assentar();

        Assert.AreEqual(1, alvo.Health, "o inimigo precisa acordar já com 1 de vida");

        kaida.SetFacing(1);
        alvo.transform.position = kaida.attackPoint.position;
        alvo.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        kaida.DoAttackHit();
        yield return null;

        Assert.IsTrue(alvo.Dying, "com 1 de vida um golpe deveria matar");
    }

    [UnityTest]
    public IEnumerator InimigoMorto_ParaDeColidir()
    {
        var alvo = CenarioDeTeste.CriarInimigo<EnemyController>(new Vector2(2f, 1f),
                                                               configurar: e => e.maxHealth = 1);
        yield return null;
        yield return Assentar(0.3f);

        alvo.TakeDamage(5, Vector2.zero);
        yield return null;

        foreach (var c in alvo.GetComponentsInChildren<Collider2D>())
            Assert.IsFalse(c.enabled, "o cadáver não pode continuar empurrando a Kaida");
    }

    [UnityTest]
    public IEnumerator Caracol_FicaImuneEnquantoEscondido()
    {
        var caracol = CenarioDeTeste.CriarInimigo<SnailEnemy>(new Vector2(3f, 1f), configurar: e =>
        {
            e.maxHealth = 6;
            e.hitsBeforeHiding = 1;
        });
        yield return null;
        yield return Assentar(0.3f);

        caracol.TakeDamage(1, Vector2.zero);       // se esconde
        yield return null;
        Assert.IsTrue(caracol.IsHidden, "depois do primeiro golpe ele entra na casca");

        int vidaNaCasca = caracol.Health;
        caracol.TakeDamage(3, Vector2.zero);
        yield return null;

        Assert.AreEqual(vidaNaCasca, caracol.Health,
            "dentro da casca não entra dano: é o que obriga o jogador a esperar o timing");
    }

    [UnityTest]
    public IEnumerator Javali_InvesteAoAvistarAKaida()
    {
        kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        var javali = CenarioDeTeste.CriarInimigo<BoarEnemy>(new Vector2(4f, 1f), configurar: e =>
        {
            e.detectRange = 8f;
            e.telegraphTime = 0.2f;
            e.chargeSpeed = 8f;
        });
        yield return null;
        yield return Assentar(0.3f);

        float xInicial = javali.transform.position.x;
        // avisa, recua e dispara
        yield return new WaitForSeconds(1.1f);

        Assert.Less(javali.transform.position.x, xInicial - 0.5f,
            "o javali deveria ter avançado na direção da Kaida");
    }

    [UnityTest]
    public IEnumerator Inimigo_NaoCaiDaBordaDaPlataforma()
    {
        CenarioDeTeste.Limpar();
        Physics2D.IgnoreLayerCollision(CenarioDeTeste.LayerPlayer, CenarioDeTeste.LayerEnemy, true);
        CenarioDeTeste.CriarSistemas();
        // plataforma curta e isolada
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(10f, 2f));

        var inimigo = CenarioDeTeste.CriarInimigo<EnemyController>(new Vector2(0f, 1f), configurar: e =>
        {
            e.moveSpeed = 4f;
            e.patrolPointA = null;
            e.patrolPointB = null;
        });
        yield return null;

        for (float t = 0f; t < 4f; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();

        Assert.Greater(inimigo.transform.position.y, -3f,
            "o inimigo andou para fora da plataforma: a checagem de borda falhou");
        Assert.Less(Mathf.Abs(inimigo.transform.position.x), 6f,
            "o inimigo deveria ter virado antes de sair da plataforma");
    }
}
