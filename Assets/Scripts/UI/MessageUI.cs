using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Caixa de texto que aparece ao pegar habilidades e fragmentos de lore.
/// Se ninguém montou a UI na cena, ela se monta sozinha - assim um coletável
/// numa cena de teste funciona sem depender de prefab.
/// </summary>
public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    Text label;
    CanvasGroup group;
    Coroutine rotina;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (label == null) Montar();
    }

    /// <summary>Mostra uma mensagem por alguns segundos. Seguro chamar de qualquer lugar.</summary>
    public static void Show(string texto, float duracao = 3.5f)
    {
        if (string.IsNullOrEmpty(texto)) return;
        if (Instance == null)
        {
            var go = new GameObject("MessageUI");
            Instance = go.AddComponent<MessageUI>();
        }
        Instance.Exibir(texto, duracao);
    }

    void Exibir(string texto, float duracao)
    {
        if (label == null) Montar();
        label.text = texto;
        if (rotina != null) StopCoroutine(rotina);
        rotina = StartCoroutine(FadeRoutine(duracao));
    }

    IEnumerator FadeRoutine(float duracao)
    {
        group.alpha = 0f;
        // entra
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / 0.25f;
            yield return null;
        }
        group.alpha = 1f;
        yield return new WaitForSecondsRealtime(duracao);
        // sai
        for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - (t / 0.4f);
            yield return null;
        }
        group.alpha = 0f;
        rotina = null;
    }

    void Montar()
    {
        UIKit.ConfigurarCanvas(gameObject, 200, comCliques: false);

        var painel = new GameObject("Painel", typeof(RectTransform));
        painel.transform.SetParent(transform, false);
        group = painel.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        var fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.04f, 0.05f, 0.08f, 0.85f);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(420f, 90f);

        var textoGO = new GameObject("Texto", typeof(RectTransform));
        textoGO.transform.SetParent(painel.transform, false);
        label = textoGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 16;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.95f, 0.93f, 0.85f);
        var trt = textoGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12f, 8f);
        trt.offsetMax = new Vector2(-12f, -8f);
    }
}
