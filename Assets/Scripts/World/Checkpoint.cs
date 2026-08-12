using UnityEngine;

/// <summary>
/// Marco de descanso. Grava a posição no GameManager e salva o progresso -
/// morrer devolve Kaida ao último marco tocado, não ao começo da região.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    public string checkpointId = "";
    public Color corInativa = new Color(0.5f, 0.5f, 0.6f);
    public Color corAtiva = new Color(1f, 0.85f, 0.5f);

    SpriteRenderer sr;
    bool ativado;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        if (sr != null) sr.color = corInativa;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ativado) return;
        if (other.GetComponent<PlayerController>() == null) return;

        ativado = true;
        if (sr != null) sr.color = corAtiva;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(transform.position,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        if (SaveSystem.Instance != null) SaveSystem.Instance.SaveGame();
    }
}
