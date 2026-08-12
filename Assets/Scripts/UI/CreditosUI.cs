using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de créditos, com a origem de tudo que não foi feito pela equipe.
///
/// É exigência da entrega: assets de terceiros podem ser usados desde que
/// sejam gratuitos ou licenciados e que a fonte apareça numa tela de
/// créditos dentro do próprio jogo — não basta citar no relatório.
/// </summary>
public class CreditosUI : MonoBehaviour
{
    [Tooltip("Integrantes da equipe, um por linha.")]
    [TextArea(2, 6)]
    public string equipe =
        "Fabrício Júnio Almeida Dias\n" +
        "Camila Pereira Raimundo\n" +
        "Luan Miranda Padilha\n" +
        "Kauã Limão Nunes";

    GameObject painel;
    Text corpo;

    void Start()
    {
        Montar();
        Fechar();
    }

    public void Abrir()
    {
        painel.SetActive(true);
        TelaModal.Abriu();
    }

    public void Fechar()
    {
        if (painel == null) return;
        if (painel.activeSelf) TelaModal.Fechou();
        painel.SetActive(false);
    }

    public bool Aberta => painel != null && painel.activeSelf;

    void Update()
    {
        if (Aberta && Input.GetKeyDown(KeyCode.Escape)) Fechar();
    }

    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 350);

        painel = new GameObject("Painel", typeof(RectTransform));
        painel.transform.SetParent(transform, false);

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.04f, 0.05f, 0.08f, 0.97f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UIKit.Titulo(painel.transform, "CRÉDITOS", 30, new Vector2(0f, 150f));

        corpo = UIKit.Paragrafo(painel.transform, MontarTexto(),
                                new Vector2(0f, 122f), new Vector2(560f, 250f), 12);
        corpo.alignment = TextAnchor.UpperCenter;
        corpo.color = UIKit.CorTexto;

        var voltar = UIKit.Coluna(painel.transform, new Vector2(0f, -128f), 180f, 8f);
        UIKit.Botao(voltar, "Voltar", Fechar);

        UIKit.Rodape(painel.transform, "Esc para voltar");
    }

    string MontarTexto()
    {
        return
            "KAIDA — RAÍZES DO ESQUECIMENTO\n" +
            "Metroidvania 2D · Unity 2022.3\n" +
            "\n" +
            "DESENVOLVIMENTO\n" +
            equipe + "\n" +
            "\n" +
            "ARTE\n" +
            "Legacy Fantasy — High Forest, de Anokolisa\n" +
            "anokolisa.itch.io · gratuito, uso comercial permitido\n" +
            "Personagem, inimigos, tiles, árvores, construções e HUD\n" +
            "\n" +
            "Stringstar Fields\n" +
            "Fundos da Caverna Musgosa e do Santuário Esquecido\n" +
            "\n" +
            "TRILHA SONORA\n" +
            "Gerada por síntese no próprio jogo, sem áudio de terceiros\n" +
            "Escala menor, arpejo e baixo sustentado, com tônica\n" +
            "diferente para cada região\n" +
            "\n" +
            "Disciplina de Desenvolvimento de Jogos Digitais\n" +
            "Ciência da Computação";
    }
}
