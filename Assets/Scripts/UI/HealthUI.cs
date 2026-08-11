using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de vida em "pips". Se não receber um container montado no Inspector,
/// cria o próprio canvas por código — assim qualquer cena mostra a vida sem
/// depender de prefab de UI.
/// Reage a HealthChanged e reconstrói os pips quando a vida máxima cresce
/// (Nódulos de Vida aumentam o total durante a partida).
/// </summary>
public class HealthUI : MonoBehaviour
{
    public PlayerController player;
    public Transform container;    // opcional: HorizontalLayoutGroup montado à mão
    public Sprite fullIcon, emptyIcon;

    public Color corCheio = new Color(1f, 0.85f, 0.45f);
    public Color corVazio = new Color(0.25f, 0.22f, 0.22f);

    Image[] pips;
    int maxAtual = -1;

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        if (container == null) container = MontarContainer();

        player.HealthChanged += UpdateUI;
        UpdateUI(player.health, player.stats.maxHealth);
    }

    void OnDestroy()
    {
        if (player != null) player.HealthChanged -= UpdateUI;
    }

    Transform MontarContainer()
    {
        var canvasGO = new GameObject("HUD");
        canvasGO.transform.SetParent(transform, false);
        UIKit.ConfigurarCanvas(canvasGO, 100, comCliques: false);

        var linha = new GameObject("Pips", typeof(RectTransform));
        linha.transform.SetParent(canvasGO.transform, false);
        var layout = linha.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;

        var rt = linha.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(16f, -16f);
        rt.sizeDelta = new Vector2(200f, 20f);

        return linha.transform;
    }

    void ReconstruirPips(int max)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        pips = new Image[max];
        for (int i = 0; i < max; i++)
        {
            var go = new GameObject("Pip" + i, typeof(RectTransform));
            go.transform.SetParent(container, false);
            var img = go.AddComponent<Image>();
            img.sprite = fullIcon;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 16f;
            le.preferredHeight = 16f;
            img.rectTransform.sizeDelta = new Vector2(16, 16);
            pips[i] = img;
        }
        maxAtual = max;
    }

    void UpdateUI(int current, int max)
    {
        if (max != maxAtual) ReconstruirPips(max);
        if (pips == null) return;

        for (int i = 0; i < pips.Length; i++)
        {
            bool cheio = i < current;

            if (fullIcon != null && emptyIcon != null)
            {
                pips[i].sprite = cheio ? fullIcon : emptyIcon;
                pips[i].color = Color.white;
            }
            else if (fullIcon != null)
            {
                // um ícone só: o pip gasto fica apagado em vez de trocar de arte
                pips[i].sprite = fullIcon;
                pips[i].color = cheio ? Color.white : corVazio;
            }
            else
            {
                pips[i].color = cheio ? corCheio : corVazio;
            }
        }
    }
}
