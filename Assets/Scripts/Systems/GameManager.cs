using UnityEngine;

/// <summary>
/// Singleton de alto nível: checkpoint atual, respawn do jogador, e ponto
/// central para os sistemas conversarem entre si.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector2 CurrentCheckpoint { get; private set; }
    public string CurrentRoom { get; private set; } = "";
    public PlayerController PlayerRef { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(PlayerController player)
    {
        PlayerRef = player;
        player.Died += OnPlayerDied;
    }

    public void SetCheckpoint(Vector2 pos, string room = "")
    {
        CurrentCheckpoint = pos;
        if (!string.IsNullOrEmpty(room)) CurrentRoom = room;
    }

    void OnPlayerDied()
    {
        Invoke(nameof(RespawnPlayer), 1.0f);
    }

    public void RespawnPlayer()
    {
        if (PlayerRef == null) return;
        PlayerRef.transform.position = CurrentCheckpoint;
        PlayerRef.health = PlayerRef.stats.maxHealth;
        PlayerRef.isInvulnerable = false;
        PlayerRef.Machine.ChangeState("idle");
    }
}
