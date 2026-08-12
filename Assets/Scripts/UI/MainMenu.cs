using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tela inicial: começar, continuar de onde parou, escolher dificuldade,
/// ver os controles e sair. É a primeira cena do jogo.
/// </summary>
public class MainMenu : MonoBehaviour
{
    enum Aba { Principal, Dificuldade, Controles, Video }

    [Tooltip("Tela de creditos, exigida pela entrega.")]
    public CreditosUI creditos;

    GameObject painelPrincipal, painelDificuldade, painelControles, painelVideo;
    Button botaoContinuar;
    Text rotuloDificuldade, rotuloTela, rotuloVolume;

    void Start()
    {
        Time.timeScale = 1f;   // pode ter voltado de um jogo pausado
        Montar();
        Trocar(Aba.Principal);
        AtualizarDificuldade();
        AtualizarContinuar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            (painelDificuldade.activeSelf || painelControles.activeSelf || painelVideo.activeSelf))
            Trocar(Aba.Principal);

        if (rotuloTela != null && painelVideo.activeSelf) AtualizarTela();
    }

    // ------------------------------------------------------------------ ações
    /// <summary>
    /// Começar uma partida passa pela escolha da dificuldade. Ela vale para a
    /// partida inteira, então perguntar aqui é mais honesto do que deixar a
    /// opção escondida num submenu que o jogador pode nem abrir.
    /// </summary>
    void NovoJogo() => Trocar(Aba.Dificuldade);

    void ComecarPartida(Dificuldade d)
    {
        GameSettings.Atual = d;

        // começar do zero significa apagar o progresso antigo de verdade
        if (SaveSystem.Instance != null) SaveSystem.Instance.DeleteSave();
        else DeletarSaveSemInstancia();

        SceneManager.LoadScene("01_OrlaDaVila");
    }

    void Continuar()
    {
        if (SaveSystem.Instance != null) SaveSystem.Instance.LoadGame();
        SceneManager.LoadScene("01_OrlaDaVila");
    }

    void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ----------------------------------------------------------------- estado
    void AtualizarContinuar()
    {
        bool existeSave = SaveSystem.Instance != null
            ? SaveSystem.Instance.HasSave()
            : System.IO.File.Exists(Application.persistentDataPath + "/savegame.json");

        if (botaoContinuar != null)
        {
            botaoContinuar.interactable = existeSave;
            var t = botaoContinuar.GetComponentInChildren<Text>();
            if (t != null) t.color = existeSave ? UIKit.CorTexto : UIKit.CorApagada;
        }
    }

    void AtualizarDificuldade()
    {
        if (rotuloDificuldade != null)
            rotuloDificuldade.text = $"Última dificuldade usada: {GameSettings.Nome(GameSettings.Atual)}";
    }

    static void DeletarSaveSemInstancia()
    {
        string caminho = Application.persistentDataPath + "/savegame.json";
        if (System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
    }

    void Trocar(Aba aba)
    {
        painelPrincipal.SetActive(aba == Aba.Principal);
        painelDificuldade.SetActive(aba == Aba.Dificuldade);
        painelControles.SetActive(aba == Aba.Controles);
        painelVideo.SetActive(aba == Aba.Video);
        if (aba == Aba.Video) { AtualizarTela(); AtualizarVolume(); }
    }

    // ------------------------------------------------------------------ tela
    /// <summary>
    /// Alterna cheia/janela. Útil na hora de apresentar: o projetor da sala
    /// costuma ter resolução diferente do monitor onde o jogo foi feito.
    /// </summary>
    void AlternarTelaCheia()
    {
        bool cheia = !Screen.fullScreen;
        var resolucao = Screen.currentResolution;

        if (cheia) Screen.SetResolution(resolucao.width, resolucao.height, FullScreenMode.FullScreenWindow);
        else       Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

        AtualizarTela();
    }

    void AtualizarTela()
    {
        if (rotuloTela == null) return;
        string modo = Screen.fullScreen ? "tela cheia" : "janela";
        float proporcao = (float)Screen.width / Mathf.Max(1, Screen.height);
        rotuloTela.text = $"Agora: {Screen.width} x {Screen.height}  ({modo})\n" +
                          $"Proporção {proporcao:0.00} - a interface se ajusta sozinha";
    }

    // -------------------------------------------------------------- interface
    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 10);

        painelPrincipal = CriarPainel("Principal");
        painelDificuldade = CriarPainel("Dificuldade");
        painelControles = CriarPainel("Controles");
        painelVideo = CriarPainel("Video");

        MontarPrincipal();
        MontarDificuldade();
        MontarControles();
        MontarVideo();
    }

    void MontarVideo()
    {
        var t = painelVideo.transform;
        UIKit.Titulo(t, "TELA E SOM", 26, new Vector2(ColunaX, 120f));

        rotuloTela = UIKit.Paragrafo(t, "", new Vector2(ColunaX, 90f), new Vector2(320f, 46f), 12);

        var coluna = UIKit.Coluna(t, new Vector2(ColunaX, 38f), 230f, 9f);
        UIKit.Botao(coluna, "Tela cheia / janela", AlternarTelaCheia);
        UIKit.Botao(coluna, "Som: menos", () => AjustarVolume(-0.25f));
        UIKit.Botao(coluna, "Som: mais", () => AjustarVolume(+0.25f));

        rotuloVolume = UIKit.Subtitulo(t, "", new Vector2(ColunaX, -80f), 13);

        UIKit.Paragrafo(t,
            "A trilha é de fundo: fica baixa de propósito,\n" +
            "para dar clima sem atrapalhar quem está jogando.",
            new Vector2(ColunaX, -96f), new Vector2(320f, 60f), 11);

        var voltar = UIKit.Coluna(t, new Vector2(ColunaX, -140f), 180f, 8f);
        UIKit.Botao(voltar, "Voltar", () => Trocar(Aba.Principal));
    }

    void AbrirCreditos()
    {
        if (creditos == null) creditos = FindObjectOfType<CreditosUI>();
        if (creditos != null) creditos.Abrir();
    }

    void AjustarVolume(float passo)
    {
        GameSettings.Volume = Mathf.Clamp01(GameSettings.Volume + passo);
        AtualizarVolume();
    }

    void AtualizarVolume()
    {
        if (rotuloVolume == null) return;
        rotuloVolume.text = $"Som da trilha: {GameSettings.NomeDoVolume()}";
    }

    /// <summary>
    /// Deslocamento horizontal do conteúdo: o menu fica à esquerda e deixa o
    /// cenário aparecer do lado direito, atrás do desfoque.
    /// </summary>
    const float ColunaX = -168f;

    GameObject CriarPainel(string nome)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Véu só do lado do menu, para o texto ter contraste sem tapar o
        // cenário inteiro. O painel em si não recebe Image: assim o fundo
        // desfocado continua visível à esquerda.
        var veu = new GameObject("Veu", typeof(RectTransform));
        veu.transform.SetParent(go.transform, false);
        var img = veu.AddComponent<Image>();
        img.color = new Color(0.04f, 0.05f, 0.08f, 0.72f);
        img.raycastTarget = false;

        var vrt = veu.GetComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0f, 0f);
        vrt.anchorMax = new Vector2(0.5f, 1f);
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;

        return go;
    }

    void MontarPrincipal()
    {
        var t = painelPrincipal.transform;
        UIKit.Titulo(t, "KAIDA", 44, new Vector2(ColunaX, 134f));
        UIKit.Subtitulo(t, "Raízes do Esquecimento", new Vector2(ColunaX, 104f), 15);

        // Seis opções, não sete: com a coluna começando em 74 cada botão come
        // 37 de altura, e a sétima passava por baixo do rótulo da dificuldade.
        var coluna = UIKit.Coluna(t, new Vector2(ColunaX, 74f), 230f, 7f);
        UIKit.Botao(coluna, "Novo jogo", NovoJogo);
        botaoContinuar = UIKit.Botao(coluna, "Continuar", Continuar);
        UIKit.Botao(coluna, "Controles", () => Trocar(Aba.Controles));
        UIKit.Botao(coluna, "Tela e som", () => Trocar(Aba.Video));
        UIKit.Botao(coluna, "Créditos", AbrirCreditos);
        UIKit.Botao(coluna, "Sair", Sair);

        rotuloDificuldade = UIKit.Subtitulo(t, "", new Vector2(ColunaX, -156f), 12);
    }

    void MontarDificuldade()
    {
        var t = painelDificuldade.transform;
        UIKit.Titulo(t, "DIFICULDADE", 26, new Vector2(ColunaX, 128f));
        UIKit.Subtitulo(t, "Vale para a partida inteira", new Vector2(ColunaX, 104f), 12);

        var coluna = UIKit.Coluna(t, new Vector2(ColunaX, 78f), 230f, 9f);
        UIKit.Botao(coluna, "Fácil", () => ComecarPartida(Dificuldade.Facil));
        UIKit.Botao(coluna, "Normal", () => ComecarPartida(Dificuldade.Normal));
        UIKit.Botao(coluna, "Difícil", () => ComecarPartida(Dificuldade.Dificil));

        // As três descrições ficam à vista de uma vez: como o clique já começa
        // a partida, não sobra momento para o jogador ler uma de cada vez.
        UIKit.Paragrafo(t,
            "Fácil - 7 de vida e recuperação longa.\n" +
            "Normal - o jogo como foi balanceado.\n" +
            "Difícil - 3 de vida e inimigos mais atentos.",
            new Vector2(ColunaX, -48f), new Vector2(310f, 60f), 12);

        var voltar = UIKit.Coluna(t, new Vector2(ColunaX, -104f), 180f, 8f);
        UIKit.Botao(voltar, "Voltar", () => Trocar(Aba.Principal));

        UIKit.Rodape(t, "Escolher já começa a partida", ColunaX);
    }

    void MontarControles()
    {
        var t = painelControles.transform;
        UIKit.Titulo(t, "CONTROLES", 28, new Vector2(ColunaX, 116f));

        UIKit.Paragrafo(t,
            "Andar          setas  ou  A / D\n" +
            "Pular          espaço\n" +
            "Atacar         Ctrl esquerdo  ou  clique\n" +
            "Dash           Shift esquerdo\n" +
            "Pausar         Esc\n\n" +
            "Pulo duplo e escalada de parede aparecem\n" +
            "quando você encontrar cada habilidade no vale.",
            new Vector2(ColunaX, 74f), new Vector2(320f, 170f), 13);

        var voltar = UIKit.Coluna(t, new Vector2(ColunaX, -118f), 180f, 8f);
        UIKit.Botao(voltar, "Voltar", () => Trocar(Aba.Principal));
    }
}
