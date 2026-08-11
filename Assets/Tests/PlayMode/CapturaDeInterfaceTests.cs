using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Grava imagens das telas com a interface montada.
///
/// Não é bem um teste: é uma forma de conferir o que o jogador enxerga.
/// A captura pela câmera (CapturaDeTela, no Editor) não pega canvas em
/// Screen Space Overlay, então menu, pausa, HUD e tela de morte ficavam
/// invisíveis na revisão. Aqui o jogo está de fato rodando, e a interface
/// existe — basta pedir o frame.
///
/// Além das imagens, cada método confere que a tela em questão realmente
/// apareceu: uma captura em branco não passaria despercebida.
/// </summary>
public class CapturaDeInterfaceTests
{
    static string Pasta => Path.Combine(Directory.GetCurrentDirectory(), "Capturas", "Interface");

    /// <summary>
    /// Estas telas congelam o tempo de propósito. Sem restaurar aqui, um teste
    /// que falhasse no meio deixaria Time.timeScale em zero e derrubaria toda
    /// a suíte seguinte — com erros que não têm relação nenhuma com a causa.
    /// </summary>
    [TearDown]
    public void Depois()
    {
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Grava o frame com a interface junto.
    ///
    /// ScreenCapture não funciona em batchmode (não há tela de verdade para
    /// copiar), e a câmera sozinha não desenha canvas em Screen Space Overlay.
    /// A saída é passar os canvas temporariamente para Screen Space Camera e
    /// renderizar a câmera numa textura — aí a interface entra no quadro.
    /// </summary>
    static void Guardar(string nome)
    {
        Directory.CreateDirectory(Pasta);

        var cam = Camera.main ?? Object.FindObjectOfType<Camera>();
        if (cam == null) { Debug.LogWarning("[Kaida] sem câmera para capturar " + nome); return; }

        var canvas = Object.FindObjectsOfType<Canvas>();
        var modosOriginais = new RenderMode[canvas.Length];
        var camerasOriginais = new Camera[canvas.Length];

        for (int i = 0; i < canvas.Length; i++)
        {
            modosOriginais[i] = canvas[i].renderMode;
            camerasOriginais[i] = canvas[i].worldCamera;
            if (canvas[i].renderMode != RenderMode.ScreenSpaceOverlay) continue;
            canvas[i].renderMode = RenderMode.ScreenSpaceCamera;
            canvas[i].worldCamera = cam;
            canvas[i].planeDistance = 1f;
        }
        Canvas.ForceUpdateCanvases();

        const int largura = 1280, altura = 540;
        var rt = new RenderTexture(largura, altura, 24);
        var alvoAnterior = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();

        var foto = new Texture2D(largura, altura, TextureFormat.RGB24, false);
        var ativoAnterior = RenderTexture.active;
        RenderTexture.active = rt;
        foto.ReadPixels(new Rect(0, 0, largura, altura), 0, 0);
        foto.Apply();
        RenderTexture.active = ativoAnterior;

        cam.targetTexture = alvoAnterior;
        File.WriteAllBytes(Path.Combine(Pasta, nome + ".png"), foto.EncodeToPNG());

        Object.Destroy(foto);
        rt.Release();
        Object.Destroy(rt);

        // devolve os canvas ao estado original: o jogo usa Overlay
        for (int i = 0; i < canvas.Length; i++)
        {
            if (canvas[i] == null) continue;
            canvas[i].renderMode = modosOriginais[i];
            canvas[i].worldCamera = camerasOriginais[i];
        }
    }

    [UnityTest]
    public IEnumerator Tela_MenuPrincipal()
    {
        SceneManager.LoadScene("00_MenuPrincipal");
        yield return null;
        for (int i = 0; i < 30; i++) yield return null;   // deixa o menu montar

        var menu = Object.FindObjectOfType<MainMenu>();
        Assert.IsNotNull(menu);

        var canvas = Object.FindObjectOfType<Canvas>();
        Assert.IsNotNull(canvas, "o menu não montou canvas nenhum");

        var botoes = Object.FindObjectsOfType<UnityEngine.UI.Button>();
        Assert.GreaterOrEqual(botoes.Length, 5,
            $"a tela inicial deveria ter ao menos 5 opções, tem {botoes.Length}");

        Guardar("01_menu_principal");
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Tela_JogoComHud()
    {
        SceneManager.LoadScene("01_OrlaDaVila");
        yield return null;
        for (int i = 0; i < 40; i++) yield return null;

        var hud = Object.FindObjectOfType<HealthUI>();
        Assert.IsNotNull(hud, "a região está sem HUD de vida");

        var pips = Object.FindObjectsOfType<UnityEngine.UI.Image>();
        Assert.Greater(pips.Length, 0, "a HUD não desenhou nenhum pip de vida");

        Guardar("02_jogo_com_hud");
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Tela_MenuDePausa()
    {
        SceneManager.LoadScene("01_OrlaDaVila");
        yield return null;
        for (int i = 0; i < 30; i++) yield return null;

        var pausa = Object.FindObjectOfType<PauseMenu>();
        Assert.IsNotNull(pausa);

        pausa.Pausar();
        Assert.IsTrue(PauseMenu.Pausado);
        yield return null;
        yield return null;

        Guardar("03_menu_de_pausa");
        yield return null;
        yield return null;

        pausa.Retomar();   // não deixa o tempo congelado para o próximo teste
    }

    [UnityTest]
    public IEnumerator Tela_DeMorte()
    {
        SceneManager.LoadScene("01_OrlaDaVila");
        yield return null;
        for (int i = 0; i < 30; i++) yield return null;

        var kaida = Object.FindObjectOfType<PlayerController>();
        Assert.IsNotNull(kaida);

        kaida.TakeDamage(999, Vector2.zero);
        Assert.AreEqual("dead", kaida.CurrentStateName);

        // a tela aparece com um respiro depois da animação de morte
        yield return new WaitForSecondsRealtime(1.6f);

        Guardar("04_tela_de_morte");
        yield return null;
        yield return null;

        Time.timeScale = 1f;
    }

    [UnityTest]
    public IEnumerator Tela_Vitoria()
    {
        SceneManager.LoadScene("05_SantuarioEsquecido");
        yield return null;
        for (int i = 0; i < 30; i++) yield return null;

        var boss = Object.FindObjectOfType<GuardianBoss>();
        Assert.IsNotNull(boss, "o Guardião não está na cena final");

        // passa a introdução e derruba as três fases
        yield return new WaitForSecondsRealtime(2.6f);
        for (int fase = 0; fase < 3; fase++)
        {
            boss.TakeDamage(999, Vector2.zero);
            yield return new WaitForSecondsRealtime(2.4f);
        }

        Assert.IsTrue(boss.Derrotado, "o chefe deveria ter caído");
        yield return new WaitForSecondsRealtime(3.6f);

        Guardar("05_tela_de_vitoria");
        yield return null;
        yield return null;

        Time.timeScale = 1f;
    }
}
