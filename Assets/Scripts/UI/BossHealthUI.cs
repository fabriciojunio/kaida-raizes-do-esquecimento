using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida do Guardião. Aparece durante o confronto e some quando
/// ele cai. Uma barra só: o combate não tem fases.
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    public GuardianBoss boss;

    RectTransform preenchimento;
    Text rotulo;
    CanvasGroup grupo;

    void Start()
    {
        if (boss == null) boss = FindObjectOfType<GuardianBoss>();
        Montar();
        if (boss == null) { grupo.alpha = 0f; return; }

        boss.HealthChanged += AoMudarVida;
        boss.Morreu += AoMorrer;
        AoMudarVida(boss.Health, boss.maxHealth);
        if (rotulo != null) rotulo.text = "O Guardião do Lúmen";
    }

    void OnDestroy()
    {
        if (boss == null) return;
        boss.HealthChanged -= AoMudarVida;
        boss.Morreu -= AoMorrer;
    }

    /// <summary>
    /// Encolhe a barra pela largura do próprio retângulo.
    ///
    /// Antes isto usava Image.fillAmount, que só tem efeito quando a Image
    /// tem um sprite. A barra é desenhada por código, sem sprite nenhum, e
    /// por isso aparecia sempre cheia: dava para bater no chefe a luta
    /// inteira sem ver um pixel de diferença.
    /// </summary>
    void AoMudarVida(int atual, int max)
    {
        if (preenchimento == null) return;
        float fracao = max > 0 ? Mathf.Clamp01((float)atual / max) : 0f;
        preenchimento.anchorMin = new Vector2(0f, 0f);
        preenchimento.anchorMax = new Vector2(fracao, 1f);
    }

    void AoMorrer()
    {
        if (grupo != null) grupo.alpha = 0f;
    }

    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 120, comCliques: false);

        var painel = new GameObject("BarraChefe", typeof(RectTransform));
        painel.transform.SetParent(transform, false);
        grupo = painel.AddComponent<CanvasGroup>();
        var prt = painel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0f);
        prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.anchoredPosition = new Vector2(0f, 22f);
        prt.sizeDelta = new Vector2(380f, 26f);

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.06f, 0.06f, 0.09f, 0.85f);

        // trilho fixo, para a barra ter contra o que encolher
        var trilhoGO = new GameObject("Trilho", typeof(RectTransform));
        trilhoGO.transform.SetParent(painel.transform, false);
        var trilhoImg = trilhoGO.AddComponent<Image>();
        trilhoImg.color = new Color(0.16f, 0.13f, 0.10f, 0.9f);
        var trt2 = trilhoGO.GetComponent<RectTransform>();
        trt2.anchorMin = Vector2.zero;
        trt2.anchorMax = Vector2.one;
        trt2.offsetMin = new Vector2(3f, 3f);
        trt2.offsetMax = new Vector2(-3f, -3f);

        var barraGO = new GameObject("Preenchimento", typeof(RectTransform));
        barraGO.transform.SetParent(trilhoGO.transform, false);
        var barraImg = barraGO.AddComponent<Image>();
        barraImg.color = new Color(0.95f, 0.75f, 0.35f);
        preenchimento = barraGO.GetComponent<RectTransform>();
        preenchimento.anchorMin = Vector2.zero;
        preenchimento.anchorMax = Vector2.one;
        preenchimento.offsetMin = Vector2.zero;
        preenchimento.offsetMax = Vector2.zero;

        var textoGO = new GameObject("Rotulo", typeof(RectTransform));
        textoGO.transform.SetParent(painel.transform, false);
        rotulo = textoGO.AddComponent<Text>();
        rotulo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rotulo.fontSize = 12;
        rotulo.alignment = TextAnchor.MiddleCenter;
        rotulo.color = new Color(0.9f, 0.88f, 0.82f);
        var trt = textoGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 0f);
        trt.anchoredPosition = new Vector2(0f, 3f);
        trt.sizeDelta = new Vector2(0f, 16f);
    }
}
