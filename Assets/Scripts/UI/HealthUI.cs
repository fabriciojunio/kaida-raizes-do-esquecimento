using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD simples de vida em "pips" (ícones). Arraste um prefab de ícone
/// (Image) e o container (HorizontalLayoutGroup) no Inspector, ou deixe
/// que o script gere os ícones por código (fallback simples com Image branca).
/// </summary>
public class HealthUI : MonoBehaviour
{
    public PlayerController player;
    public Transform container;   // HorizontalLayoutGroup vazio
    public Sprite fullIcon, emptyIcon;

    Image[] pips;

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (player == null || container == null) return;

        int max = player.stats.maxHealth;
        pips = new Image[max];
        for (int i = 0; i < max; i++)
        {
            var go = new GameObject("Pip" + i);
            go.transform.SetParent(container, false);
            var img = go.AddComponent<Image>();
            img.sprite = fullIcon;
            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(20, 20);
            pips[i] = img;
        }
        player.HealthChanged += UpdateUI;
        UpdateUI(player.health, max);
    }

    void UpdateUI(int current, int max)
    {
        if (pips == null) return;
        for (int i = 0; i < pips.Length; i++)
            pips[i].sprite = (i < current) ? fullIcon : emptyIcon;
    }
}
