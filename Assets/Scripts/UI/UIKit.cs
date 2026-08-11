using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Peças de interface montadas por código, na identidade do jogo: fundo
/// escuro, texto cor de osso, destaque cor de lúmen.
///
/// Os menus são construídos assim, e não como prefabs, para que qualquer
/// cena funcione sozinha — inclusive as cenas de teste, que não têm UI
/// montada à mão.
/// </summary>
public static class UIKit
{
    public static readonly Color CorTexto = new Color(0.90f, 0.88f, 0.82f);
    public static readonly Color CorApagada = new Color(0.55f, 0.54f, 0.52f);
    public static readonly Color CorLumen = new Color(1f, 0.85f, 0.45f);
    public static readonly Color CorBotao = new Color(0.10f, 0.11f, 0.15f, 0.95f);
    public static readonly Color CorBotaoSobre = new Color(0.18f, 0.19f, 0.24f, 1f);

    public static Font Fonte => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    /// <summary>
    /// Canvas configurado para caber em qualquer tela.
    ///
    /// O ponto crítico é o matchWidthOrHeight: no valor padrão (0) o canvas
    /// só acompanha a largura, então em monitores mais largos que 16:9 o
    /// conteúdo cresce e as opções somem para fora da tela.
    /// </summary>
    public static Canvas ConfigurarCanvas(GameObject alvo, int ordem, bool comCliques = true)
    {
        var canvas = alvo.GetComponent<Canvas>();
        if (canvas == null) canvas = alvo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordem;

        var escala = alvo.GetComponent<CanvasScaler>();
        if (escala == null) escala = alvo.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(640, 360);
        escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // Acompanha a ALTURA (1), não a largura. Num monitor ultrawide como
        // 2560x1080 o casamento por largura aumenta a interface até ela
        // estourar em cima e embaixo; pela altura ela mantém o tamanho e
        // apenas sobra espaço nas laterais, que é inofensivo.
        escala.matchWidthOrHeight = 1f;
        escala.referencePixelsPerUnit = 100f;

        if (comCliques)
        {
            if (alvo.GetComponent<GraphicRaycaster>() == null)
                alvo.AddComponent<GraphicRaycaster>();
            GarantirEventSystem();
        }
        return canvas;
    }

    /// <summary>Sem EventSystem na cena, nenhum botão responde a clique.</summary>
    public static void GarantirEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    public static Text Titulo(Transform pai, string texto, int tamanho, Vector2 posicao)
    {
        var go = new GameObject("Titulo", typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var t = go.AddComponent<Text>();
        t.font = Fonte;
        t.fontSize = tamanho;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = CorLumen;
        t.text = texto;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(520f, tamanho + 14f);
        return t;
    }

    public static Text Subtitulo(Transform pai, string texto, Vector2 posicao, int tamanho = 13)
    {
        var t = Titulo(pai, texto, tamanho, posicao);
        t.name = "Subtitulo";
        t.color = CorApagada;
        return t;
    }

    /// <summary>Coluna centralizada que empilha os botões.</summary>
    public static Transform Coluna(Transform pai, Vector2 posicao, float largura, float espaco)
    {
        var go = new GameObject("Coluna", typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = espaco;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childAlignment = TextAnchor.UpperCenter;

        var ajuste = go.AddComponent<ContentSizeFitter>();
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(largura, 0f);
        return go.transform;
    }

    public static Button Botao(Transform pai, string rotulo, UnityEngine.Events.UnityAction aoClicar)
    {
        var go = new GameObject("Botao_" + rotulo, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var fundo = go.AddComponent<Image>();
        fundo.color = CorBotao;

        var b = go.AddComponent<Button>();
        b.targetGraphic = fundo;

        var cores = b.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1.5f, 1.5f, 1.6f);
        cores.pressedColor = new Color(0.8f, 0.8f, 0.9f);
        cores.fadeDuration = 0.08f;
        b.colors = cores;

        if (aoClicar != null) b.onClick.AddListener(aoClicar);

        // 30 de altura, não 34: com seis opções na tela inicial a coluna
        // passava da borda de baixo e a última sumia
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 30f;

        var textoGO = new GameObject("Texto", typeof(RectTransform));
        textoGO.transform.SetParent(go.transform, false);
        var t = textoGO.AddComponent<Text>();
        t.font = Fonte;
        t.fontSize = 16;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = CorTexto;
        t.text = rotulo;

        var trt = textoGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return b;
    }

    public static Text Rodape(Transform pai, string texto)
    {
        var go = new GameObject("Rodape", typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var t = go.AddComponent<Text>();
        t.font = Fonte;
        t.fontSize = 12;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = CorApagada;
        t.text = texto;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 14f);
        rt.sizeDelta = new Vector2(560f, 18f);
        return t;
    }

    /// <summary>Bloco de texto livre, ancorado onde for pedido.</summary>
    public static Text Paragrafo(Transform pai, string texto, Vector2 posicao, Vector2 tamanho, int fonte = 13)
    {
        var go = new GameObject("Paragrafo", typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var t = go.AddComponent<Text>();
        t.font = Fonte;
        t.fontSize = fonte;
        t.alignment = TextAnchor.UpperCenter;
        t.color = CorApagada;
        t.text = texto;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        return t;
    }
}
