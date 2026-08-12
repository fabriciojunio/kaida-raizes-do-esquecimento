using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tela de morte. Assume o controle do respawn: enquanto ela existe na cena,
/// o GameManager não devolve o jogador sozinho - quem decide é quem está
/// jogando.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    CanvasGroup grupo;
    Text contador;
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
        if (contador != null)
        {
            contador.text = mortesNaSessao == 1
                ? "Primeira queda."
                : $"Quedas nesta sessão: {mortesNaSessao}";
        }
        Invoke(nameof(Mostrar), 0.9f);   // deixa a animação de morte respirar
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

        var titulo = UIKit.Titulo(painel.transform, "VOCÊ CAIU", 34, new Vector2(0f, 88f));
        titulo.color = new Color(0.85f, 0.35f, 0.35f);

        UIKit.Subtitulo(painel.transform, "O vale continua sem lembrar de você.", new Vector2(0f, 56f));
        contador = UIKit.Subtitulo(painel.transform, "", new Vector2(0f, 36f), 12);

        var coluna = UIKit.Coluna(painel.transform, new Vector2(0f, 0f), 220f, 10f);
        UIKit.Botao(coluna, "Voltar ao último marco", Continuar);
        UIKit.Botao(coluna, "Menu principal", VoltarAoMenu);
    }
}
