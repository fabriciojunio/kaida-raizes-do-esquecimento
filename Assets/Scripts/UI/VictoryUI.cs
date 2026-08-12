using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fim de jogo. Aparece quando o Guardião cai, com o balanço do que Kaida
/// juntou pelo caminho - os fragmentos são opcionais, então o número diz
/// quanto da história o jogador viu.
/// </summary>
public class VictoryUI : MonoBehaviour
{
    public GuardianBoss boss;

    CanvasGroup grupo;
    Text resumo;

    void Start()
    {
        Montar();
        Esconder();

        if (boss == null) boss = FindObjectOfType<GuardianBoss>();
        if (boss != null) boss.Morreu += AoVencer;
    }

    void OnDestroy()
    {
        if (boss != null) boss.Morreu -= AoVencer;
        Time.timeScale = 1f;
    }

    void AoVencer()
    {
        Invoke(nameof(Mostrar), 3.2f);   // deixa a morte do chefe terminar
    }

    void Mostrar()
    {
        TelaModal.Abriu();
        if (resumo != null) resumo.text = MontarResumo();
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

    string MontarResumo()
    {
        if (SaveSystem.Instance == null) return "";

        var dados = SaveSystem.Instance.Data;
        int fragmentos = 0, nodulos = 0;
        foreach (var id in dados.collectedItems)
        {
            if (id.StartsWith("frag_")) fragmentos++;
            else if (id.StartsWith("node_")) nodulos++;
        }

        int habilidades = dados.unlockedAbilities.Count;
        string dificuldade = GameSettings.Nome(GameSettings.Atual);

        return $"Fragmentos de Lúmen encontrados: {fragmentos}\n" +
               $"Nódulos de Vida reunidos: {nodulos}\n" +
               $"Habilidades despertadas: {habilidades} de 2\n\n" +
               $"Dificuldade: {dificuldade}";
    }

    void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("00_MenuPrincipal");
    }

    void JogarDeNovo()
    {
        Time.timeScale = 1f;
        if (SaveSystem.Instance != null) SaveSystem.Instance.DeleteSave();
        SceneManager.LoadScene("01_OrlaDaVila");
    }

    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 340);

        var painel = new GameObject("Painel", typeof(RectTransform));
        painel.transform.SetParent(transform, false);
        grupo = painel.AddComponent<CanvasGroup>();

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UIKit.Titulo(painel.transform, "O VALE VOLTA A LEMBRAR", 26, new Vector2(0f, 126f));

        // parágrafo, não subtítulo: o subtítulo é de uma linha só e cortaria
        // a segunda frase
        UIKit.Paragrafo(painel.transform,
            "O jogo não diz se Kaida o deteve\nou se lembrou de si mesma através dele.",
            new Vector2(0f, 100f), new Vector2(460f, 44f), 13);

        resumo = UIKit.Paragrafo(painel.transform, "", new Vector2(0f, 44f), new Vector2(360f, 90f));
        resumo.alignment = TextAnchor.UpperCenter;
        resumo.color = UIKit.CorTexto;

        var coluna = UIKit.Coluna(painel.transform, new Vector2(0f, -58f), 220f, 10f);
        UIKit.Botao(coluna, "Jogar de novo", JogarDeNovo);
        UIKit.Botao(coluna, "Menu principal", VoltarAoMenu);

        UIKit.Rodape(painel.transform, "Obrigado por jogar");
    }
}
