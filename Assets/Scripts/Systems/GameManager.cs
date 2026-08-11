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

    /// <summary>
    /// Quando existe uma tela de game over na cena, ela desliga isto e passa
    /// a decidir quando o jogador volta.
    /// </summary>
    public bool respawnAutomatico = true;

    /// <summary>Avisa quem estiver interessado (a tela de morte, por exemplo).</summary>
    public event System.Action PlayerMorreu;

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
        PlayerMorreu?.Invoke();
        if (respawnAutomatico) Invoke(nameof(RespawnPlayer), 1.0f);
    }

    public void RespawnPlayer()
    {
        if (PlayerRef == null) return;
        PlayerRef.transform.position = CurrentCheckpoint;
        PlayerRef.SetVelocity(0f, 0f);                 // senão o jogador reaparece caindo
        PlayerRef.health = PlayerRef.stats.maxHealth;
        PlayerRef.CancelInvulnWindow();
        PlayerRef.RefreshAirAbilities();
        PlayerRef.NotifyHealthChanged();               // sem isso a HUD fica zerada após morrer
        PlayerRef.Machine.ChangeState("idle");
    }
}
