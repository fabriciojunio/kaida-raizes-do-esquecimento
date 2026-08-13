using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tela de morte. Assume o controle do respawn: enquanto ela existe na cena,
/// o GameManager não devolve o jogador sozinho - quem decide é quem está
/// jogando.
///
/// São três tentativas por partida. Enquanto sobrar alguma, voltar ao último
/// marco mantém tudo: vida cheia, habilidades e itens. Esgotadas as três, a
/// única saída é recomeçar do início, e o save é apagado junto.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    CanvasGroup grupo;
    Text contador, titulo;
    Transform coluna;
    PlayerController jogador;

    static int mortesNaSessao;

    void Start()
    {
        Montar();
        Esconder();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.respawnAutomatico = false;   // esta tela assume
            GameManager.Instance.PlayerMorreu += AoMorrer;
        }
        jogador = FindObjectOfType<PlayerController>();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerMorreu -= AoMorrer;
        Time.timeScale = 1f;
    }

    void AoMorrer()
    {
        mortesNaSessao++;
        int restantes = GameManager.Instance != null ? GameManager.Instance.VidasRestantes : 0;
        bool acabou = restantes <= 0;

        if (titulo != null)
            titulo.text = acabou ? "FIM DA JORNADA" : "VOCÊ CAIU";

        if (contador != null)
        {
            contador.text = acabou
                ? "As três tentativas acabaram. O vale recomeça sem você."
                : (restantes == 1
                    ? "Resta uma tentativa."
                    : $"Restam {restantes} tentativas.");
        }

        MontarOpcoes(acabou);
        Invoke(nameof(Mostrar), 0.9f);   // deixa a animação de morte respirar
    }

    /// <summary>
    /// As opções mudam conforme sobra tentativa. Sem isso, o botão de voltar
    /// ao marco continuaria ali depois da terceira queda, e ele devolveria o
    /// jogador ao jogo como se nada tivesse acontecido.
    /// </summary>
    void MontarOpcoes(bool acabou)
    {
        if (coluna == null) return;
        for (int i = coluna.childCount - 1; i >= 0; i--)
            Destroy(coluna.GetChild(i).gameObject);

        if (acabou)
        {
            UIKit.Botao(coluna, "Recomeçar do início", RecomecarDoInicio);
            UIKit.Botao(coluna, "Menu principal", VoltarAoMenu);
        }
        else
        {
            UIKit.Botao(coluna, "Voltar ao último marco", Continuar);
            UIKit.Botao(coluna, "Menu principal", VoltarAoMenu);
        }
    }

    /// <summary>Partida do zero: apaga o progresso e devolve à primeira região.</summary>
    void RecomecarDoInicio()
    {
        TelaModal.Fechou();
        Time.timeScale = 1f;
        if (SaveSystem.Instance != null) SaveSystem.Instance.DeleteSave();
        if (GameManager.Instance != null) GameManager.Instance.ReiniciarVidas();
        SceneManager.LoadScene("01_OrlaDaVila");
    }

    void Continuar()
    {
        TelaModal.Fechou();
        Esconder();
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.RespawnPlayer();
    }

    void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        if (SaveSystem.Instance != null) SaveSystem.Instance.SaveGame();
        SceneManager.LoadScene("00_MenuPrincipal");
    }

    void Mostrar()
    {
        TelaModal.Abriu();
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
        Time.timeScale = 0f;
    }

    void Esconder()
    {
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
    }

    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 320);

        var painel = new GameObject("Painel", typeof(RectTransform));
        painel.transform.SetParent(transform, false);
        grupo = painel.AddComponent<CanvasGroup>();

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.05f, 0.02f, 0.03f, 0.92f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        titulo = UIKit.Titulo(painel.transform, "VOCÊ CAIU", 34, new Vector2(0f, 88f));
        titulo.color = new Color(0.85f, 0.35f, 0.35f);

        UIKit.Subtitulo(painel.transform, "O vale continua sem lembrar de você.", new Vector2(0f, 56f));
        contador = UIKit.Subtitulo(painel.transform, "", new Vector2(0f, 36f), 12);

        coluna = UIKit.Coluna(painel.transform, new Vector2(0f, 0f), 220f, 10f);
        MontarOpcoes(false);
    }
}
