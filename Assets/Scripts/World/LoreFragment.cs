using UnityEngine;

/// <summary>
/// Fragmento de Lúmen: colecionável narrativo, opcional. Não muda a
/// jogabilidade — é onde a história de Kaida e do vale aparece, aos pedaços,
/// para quem sair da rota principal.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LoreFragment : MonoBehaviour
{
    [Tooltip("Id único do fragmento, para o save saber que já foi lido.")]
    public string fragmentId = "frag_01";

    [TextArea(3, 8)]
    public string texto = "";

    void Awake() { GetComponent<Collider2D>().isTrigger = true; }

    void Start()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsCollected(fragmentId))
            gameObject.SetActive(false);
    }

    void Update()
    {
        // pulsa devagar, como memória viva no meio da decadência
        float t = (Mathf.Sin(Time.time * 1.6f) + 1f) * 0.5f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.Lerp(new Color(1f, 0.9f, 0.6f, 0.55f), new Color(1f, 0.95f, 0.75f, 1f), t);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkCollected(fragmentId);
            SaveSystem.Instance.SaveGame();
        }
        MessageUI.Show(texto, 5.5f);
        gameObject.SetActive(false);
    }
}
