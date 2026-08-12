using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Carrega as cenas de verdade e deixa a física rodar.
///
/// É o teste que pega o erro mais grave possível neste projeto: chão sem
/// colisão. Ele não aparece em captura de tela nenhuma — o cenário fica
/// perfeito e a Kaida simplesmente atravessa o mundo no primeiro segundo.
/// Já aconteceu duas vezes durante o desenvolvimento.
///
/// As regiões são percorridas dentro de cada teste, num laço, em vez de
/// virarem casos separados: [UnityTest] não aceita [TestCase] nem
/// [ValueSource] — a combinação compila, o teste simplesmente não roda,
/// e some da contagem sem avisar.
/// </summary>
public class CenasReaisTests
{
    [TearDown]
    public void Depois()
    {
        Time.timeScale = 1f;
        CenarioDeTeste.Limpar();
    }
    static readonly string[] Regioes =
    {
        "01_OrlaDaVila",
        "02_FlorestaSilente",
        "03_LagoSilente",
        "04_CavernaMusgosa",
        "05_SantuarioEsquecido",
    };

    [UnityTest]
    public IEnumerator ChaoDeTodasAsRegioes_SustentaAKaida()
    {
        var falhas = new List<string>();

        foreach (var regiao in Regioes)
        {
            SceneManager.LoadScene(regiao);
            yield return null;
            yield return null;

            var kaida = Object.FindObjectOfType<PlayerController>();
            if (kaida == null) { falhas.Add($"{regiao}: sem a Kaida na cena"); continue; }

            float alturaInicial = kaida.transform.position.y;

            // tempo de sobra para cair, se o chão não estiver segurando
            for (float t = 0f; t < 2.5f; t += Time.fixedDeltaTime)
                yield return new WaitForFixedUpdate();

            float queda = alturaInicial - kaida.transform.position.y;

            if (queda > 6f)
                falhas.Add($"{regiao}: caiu {queda:F1} unidades — o chão não tem colisão");
            else if (!kaida.IsGrounded())
                falhas.Add($"{regiao}: não encontrou chão embaixo dela");
        }

        Assert.IsEmpty(falhas, string.Join(" | ", falhas));
    }

    [UnityTest]
    public IEnumerator ChaoDeTodasAsRegioes_TemGeometriaDeColisao()
    {
        var falhas = new List<string>();

        foreach (var regiao in Regioes)
        {
            SceneManager.LoadScene(regiao);
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var composto = Object.FindObjectOfType<CompositeCollider2D>();
            if (composto == null) falhas.Add($"{regiao}: sem CompositeCollider2D");
            else if (composto.pathCount <= 0) falhas.Add($"{regiao}: geometria do chão vazia");
        }

        Assert.IsEmpty(falhas, string.Join(" | ", falhas));
    }

    [UnityTest]
    public IEnumerator TodasAsRegioes_CarregamComOEssencial()
    {
        var falhas = new List<string>();

        foreach (var regiao in Regioes)
        {
            SceneManager.LoadScene(regiao);
            yield return null;
            yield return null;

            if (SceneManager.GetActiveScene().name != regiao)
                { falhas.Add($"{regiao}: não virou a cena ativa"); continue; }

            if (Object.FindObjectOfType<Camera>() == null) falhas.Add($"{regiao}: sem câmera");
            if (Object.FindObjectOfType<GameManager>() == null) falhas.Add($"{regiao}: sem GameManager");
            if (Object.FindObjectOfType<PauseMenu>() == null) falhas.Add($"{regiao}: sem menu de pausa");
            if (Object.FindObjectOfType<TrilhaSonora>() == null) falhas.Add($"{regiao}: sem trilha");
        }

        Assert.IsEmpty(falhas, string.Join(" | ", falhas));
    }

    [UnityTest]
    public IEnumerator MenuPrincipal_CarregaComCenarioETrilha()
    {
        SceneManager.LoadScene("00_MenuPrincipal");
        yield return null;
        yield return null;

        Assert.IsNotNull(Object.FindObjectOfType<MainMenu>(), "o menu não está na cena");
        Assert.IsNotNull(Object.FindObjectOfType<KaidaDeVitrine>(),
            "o fundo do menu deveria ter a Kaida andando");
        Assert.IsNotNull(Object.FindObjectOfType<TrilhaSonora>(), "o menu está sem trilha");

        var fonte = Object.FindObjectOfType<AudioSource>();
        Assert.IsNotNull(fonte, "sem AudioSource");
        Assert.IsNotNull(fonte.clip, "a trilha não gerou áudio nenhum");
        Assert.Greater(fonte.clip.length, 5f, "a trilha ficou curta demais para um loop");
    }

    [UnityTest]
    public IEnumerator Trilha_SobreviveATrocaDeCena()
    {
        SceneManager.LoadScene("00_MenuPrincipal");
        yield return null;
        yield return null;

        var trilha = Object.FindObjectOfType<TrilhaSonora>();
        Assert.IsNotNull(trilha);
        int idOriginal = trilha.GetInstanceID();

        SceneManager.LoadScene("01_OrlaDaVila");
        yield return null;
        yield return null;

        var depois = Object.FindObjectOfType<TrilhaSonora>();
        Assert.IsNotNull(depois, "a trilha sumiu ao trocar de região");
        Assert.AreEqual(idOriginal, depois.GetInstanceID(),
            "a música deveria continuar tocando, não recomeçar do zero a cada cena");
    }
}
