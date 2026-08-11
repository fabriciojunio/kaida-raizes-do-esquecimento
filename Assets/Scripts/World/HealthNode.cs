using UnityEngine;

/// <summary>
/// Nódulo de Vida: aumenta a vida máxima em 1 pip permanentemente.
/// São 4 espalhados pelo vale, todos fora da rota principal.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HealthNode : MonoBehaviour
{
    public string nodeId = "node_01";
    [TextArea] public string mensagem = "Nódulo de Vida\nVocê aguenta um golpe a mais.";

    void Awake() { GetComponent<Collider2D>().isTrigger = true; }

    void Start()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsCollected(nodeId))
            gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;

        p.stats.maxHealth += 1;
        p.Heal(1);
        p.NotifyHealthChanged();

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkCollected(nodeId);
            SaveSystem.Instance.SaveGame();
        }
        MessageUI.Show(mensagem);
        gameObject.SetActive(false);
    }
}
