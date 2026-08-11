using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Borda de sala: leva o jogador para outra cena e o posiciona no ponto de
/// entrada correspondente. É o que costura as regiões num mapa interconectado
/// em vez de fases soltas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomTransition : MonoBehaviour
{
    [Tooltip("Nome da cena de destino, exatamente como está em Build Settings.")]
    public string targetScene;

    [Tooltip("Identificador do ponto de chegada na cena de destino (SpawnPoint.id).")]
    public string targetSpawnId = "default";

    [Tooltip("Habilidade necessária para passar. Vazio = passagem livre.")]
    public string requiredAbility = "";

    bool traveling = false;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (traveling) return;
        if (other.GetComponent<PlayerController>() == null) return;
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning($"RoomTransition em '{name}' está sem cena de destino.");
            return;
        }
        if (!string.IsNullOrEmpty(requiredAbility) &&
            SaveSystem.Instance != null && !SaveSystem.Instance.HasAbility(requiredAbility))
            return;

        traveling = true;
        SceneLoader.LoadRoom(targetScene, targetSpawnId);
    }
}
