using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// O golpe e a detecção de parede precisam funcionar dos dois lados.
///
/// Virar de lado troca só o flipX do sprite, e os marcadores presos à Kaida
/// (AttackPoint) continuavam parados à direita dela. Virada para a
/// esquerda, a caixa do golpe ficava atrás das costas: só acertava quem
/// estivesse praticamente embaixo dela.
///
/// A suíte antiga não pegou porque todo teste de combate posicionava o
/// inimigo à direita.
/// </summary>
public class GolpeParaOsDoisLadosTests
{
    [SetUp]
    public void Antes()
    {
        CenarioDeTeste.PrepararAmbiente();
        Physics2D.IgnoreLayerCollision(CenarioDeTeste.LayerPlayer, CenarioDeTeste.LayerEnemy, true);
        CenarioDeTeste.CriarSistemas();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));
    }

    [TearDown]
    public void Depois() => CenarioDeTeste.Limpar();

    [UnityTest]
    public IEnumerator OGolpe_AlcancaInimigoDosDoisLados()
    {
        foreach (int lado in new[] { 1, -1 })
        {
            var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
            var alvo = CenarioDeTeste.CriarInimigo<BoarEnemy>(
                new Vector2(lado * 1.6f, 1f), false, e => e.maxHealth = 5);

            yield return null;
            kaida.SetFacing(lado);
            yield return new WaitForFixedUpdate();

            int antes = alvo.Health;
            kaida.DoAttackHit();
            yield return null;

            Assert.Less(alvo.Health, antes,
                $"virada para {(lado > 0 ? "a direita" : "a esquerda")}, o golpe não " +
                $"alcançou o inimigo a {Mathf.Abs(lado * 1.6f)} unidade de distância. " +
                $"Kaida em {kaida.transform.position}, alvo em {alvo.transform.position}.");

            CenarioDeTeste.Limpar();
            CenarioDeTeste.CriarSistemas();
            CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));
        }
    }

    [UnityTest]
    public IEnumerator OGolpe_NaoAlcancaQuemEstaAtras()
    {
        // O espelhamento não pode virar um golpe que acerta dos dois lados de
        // uma vez: isso tiraria o sentido de virar para o inimigo.
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        var atras = CenarioDeTeste.CriarInimigo<BoarEnemy>(
            new Vector2(-2.4f, 1f), false, e => e.maxHealth = 5);

        yield return null;
        kaida.SetFacing(1);           // encara a direita, o inimigo está à esquerda
        yield return new WaitForFixedUpdate();

        int antes = atras.Health;
        kaida.DoAttackHit();
        yield return null;

        Assert.AreEqual(antes, atras.Health,
            "o golpe acertou quem estava nas costas da Kaida");
    }

    [UnityTest]
    public IEnumerator CadaTipoDeInimigo_MorreLevandoGolpe()
    {
        // Cobre os três tipos de uma vez: nenhum pode ser imortal. O caracol é
        // o caso delicado, porque fecha a casca e fica imune por um tempo.
        yield return MatarComGolpes<BoarEnemy>("javali", 12);
        yield return MatarComGolpes<BeeEnemy>("abelha", 12);
        yield return MatarComGolpes<SnailEnemy>("caracol", 40);
    }

    IEnumerator MatarComGolpes<T>(string nome, int tentativasMaximas) where T : EnemyController
    {
        CenarioDeTeste.Limpar();
        CenarioDeTeste.CriarSistemas();
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(80f, 2f));

        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 1f));
        var alvo = CenarioDeTeste.CriarInimigo<T>(new Vector2(1.6f, 1f), typeof(T) == typeof(BeeEnemy));
        yield return null;
        kaida.SetFacing(1);

        int tentativas = 0;
        while (alvo != null && !alvo.Dying && tentativas < tentativasMaximas)
        {
            kaida.DoAttackHit();
            tentativas++;
            // meio segundo entre golpes: é o ritmo de quem está jogando, e dá
            // tempo de a casca do caracol abrir de novo
            for (float t = 0f; t < 0.5f; t += Time.fixedDeltaTime)
                yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(alvo == null || alvo.Dying,
            $"o {nome} sobreviveu a {tentativas} golpes seguidos. " +
            "Inimigo que não morre trava o jogador no lugar.");
    }
}
