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

    // ------------------------------------------------------------------ vidas
    /// <summary>
    /// Tentativas por partida. Morrer devolve ao último marco com a vida
    /// cheia e o progresso intacto; esgotar as três recomeça o jogo do zero.
    ///
    /// É o que dá peso à morte sem punir tentativa: o jogador erra à vontade
    /// dentro de uma vida, mas sabe que elas acabam.
    /// </summary>
    public const int VidasPorPartida = 3;

    public int VidasRestantes { get; private set; } = VidasPorPartida;

    /// <summary>Dispara com quantas tentativas sobraram.</summary>
    public event System.Action<int> VidasMudaram;

    /// <summary>Zera o contador. Chamado ao começar uma partida nova.</summary>
    public void ReiniciarVidas()
    {
        VidasRestantes = VidasPorPartida;
        VidasMudaram?.Invoke(VidasRestantes);
    }

    /// <summary>Gasta uma tentativa. Falso quando não sobrou nenhuma.</summary>
    public bool ConsumirVida()
    {
        VidasRestantes = Mathf.Max(0, VidasRestantes - 1);
        VidasMudaram?.Invoke(VidasRestantes);
        return VidasRestantes > 0;
    }

    public bool AcabaramAsVidas => VidasRestantes <= 0;

    /// <summary>
    /// Passa a acompanhar este jogador.
    ///
    /// Registrar duas vezes não pode contar a morte duas vezes: o GameManager
    /// atravessa as trocas de cena, e cada região nova instancia uma Kaida que
    /// se registra sozinha. Sem desinscrever antes, uma queda gastava duas das
    /// três tentativas.
    /// </summary>
    public void RegisterPlayer(PlayerController player)
    {
        if (player == null) return;

        if (PlayerRef != null) PlayerRef.Died -= OnPlayerDied;
        player.Died -= OnPlayerDied;

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
        ConsumirVida();
        PlayerMorreu?.Invoke();
        // sem tela de morte na cena (testes, por exemplo) o respawn é automático
        if (respawnAutomatico && !AcabaramAsVidas) Invoke(nameof(RespawnPlayer), 1.0f);
    }

    public void RespawnPlayer()
    {
        if (PlayerRef == null) return;

        // Sem tentativa não se volta. O respawn é agendado no momento da morte,
        // e sem esta guarda um agendamento feito enquanto ainda havia tentativa
        // devolvia o jogador ao jogo depois de a última ter acabado.
        if (AcabaramAsVidas) return;
        PlayerRef.transform.position = CurrentCheckpoint;
        PlayerRef.SetVelocity(0f, 0f);                 // senão o jogador reaparece caindo
        PlayerRef.health = PlayerRef.stats.maxHealth;
        PlayerRef.CancelInvulnWindow();
        PlayerRef.RefreshAirAbilities();
        PlayerRef.NotifyHealthChanged();               // sem isso a HUD fica zerada após morrer
        PlayerRef.Machine.ChangeState("idle");
    }
}
