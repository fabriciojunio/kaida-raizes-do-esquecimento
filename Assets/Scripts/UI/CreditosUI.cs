using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de créditos, com a origem de tudo que não foi feito pela equipe.
///
/// É exigência da entrega: assets de terceiros podem ser usados desde que
/// sejam gratuitos ou licenciados e que a fonte apareça numa tela de
/// créditos dentro do próprio jogo - não basta citar no relatório.
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
        // opaco de vez: com 0,97 o título do menu aparecia por trás do texto
        fundo.color = new Color(0.04f, 0.05f, 0.08f, 1f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UIKit.Titulo(painel.transform, "CRÉDITOS", 24, new Vector2(0f, 156f));

        // A tela toda tem 360 de altura de referência. Do fim do título até o
        // topo do botão sobram cerca de 250, e o texto precisa caber aí: em 12
        // ele passava de 350 e escorria por cima do "Voltar".
        corpo = UIKit.Paragrafo(painel.transform, MontarTexto(),
                                new Vector2(0f, 132f), new Vector2(600f, 244f), 11);
        corpo.alignment = TextAnchor.UpperCenter;
        corpo.color = UIKit.CorTexto;

        var voltar = UIKit.Coluna(painel.transform, new Vector2(0f, -112f), 180f, 8f);
        UIKit.Botao(voltar, "Voltar", Fechar);

        UIKit.Rodape(painel.transform, "Esc para voltar");
    }

    string MontarTexto()
    {
        return
            "Metroidvania 2D · Unity 2022.3\n" +
            "Desenvolvimento de Jogos Digitais · Ciência da Computação\n" +
            "\n" +
            "DESENVOLVIMENTO\n" +
            equipe + "\n" +
            "\n" +
            "ARTE\n" +
            "Legacy Fantasy - High Forest, de Anokolisa (anokolisa.itch.io)\n" +
            "Gratuito, uso comercial permitido. Personagem, inimigos,\n" +
            "tiles, árvores, construções e ícones da interface.\n" +
            "Stringstar Fields - fundos da Caverna e do Santuário.\n" +
            "\n" +
            "TRILHA SONORA\n" +
            "Gerada por síntese no próprio jogo, sem áudio de terceiros.";
    }
}
