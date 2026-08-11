using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu de pausa (Esc). Congela o tempo, mostra as opções e devolve o
/// controle. Monta a própria interface por código, no mesmo padrão do resto
/// da UI do jogo, para não depender de prefab montado à mão.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool Pausado { get; private set; }

    CanvasGroup grupo;
    GameObject painel;

    void Awake()
    {
        Montar();
        Esconder();
        Pausado = false;
    }

    void OnDestroy()
    {
        // sair da cena com o jogo pausado deixaria o tempo congelado
        Time.timeScale = 1f;
        Pausado = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Pausado) Retomar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        Pausado = true;
        Time.timeScale = 0f;
        Mostrar();
    }

    public void Retomar()
    {
        Pausado = false;
        Time.timeScale = 1f;
        Esconder();
    }

    void ReiniciarRegiao()
    {
        Time.timeScale = 1f;
        Pausado = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        Pausado = false;
        if (SaveSystem.Instance != null) SaveSystem.Instance.SaveGame();
        SceneManager.LoadScene("00_MenuPrincipal");
    }

    void Sair()
    {
        if (SaveSystem.Instance != null) SaveSystem.Instance.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Mostrar()
    {
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    void Esconder()
    {
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
    }

    // ------------------------------------------------------------- interface
    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 300);   // acima da HUD e das mensagens

        painel = new GameObject("Painel", typeof(RectTransform));
        painel.transform.SetParent(transform, false);
        grupo = painel.AddComponent<CanvasGroup>();

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.03f, 0.04f, 0.06f, 0.88f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UIKit.Titulo(painel.transform, "PAUSA", 34, new Vector2(0f, 112f));

        var coluna = UIKit.Coluna(painel.transform, new Vector2(0f, 62f), 210f, 8f);
        UIKit.Botao(coluna, "Continuar", Retomar);
        UIKit.Botao(coluna, "Reiniciar região", ReiniciarRegiao);
        UIKit.Botao(coluna, "Menu principal", VoltarAoMenu);
        UIKit.Botao(coluna, "Sair do jogo", Sair);

        UIKit.Rodape(painel.transform, "Esc para voltar ao jogo");
    }
}
