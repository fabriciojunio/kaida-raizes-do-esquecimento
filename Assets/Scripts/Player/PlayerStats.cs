using UnityEngine;

/// <summary>
/// Configuração de movimento e combate do jogador (ScriptableObject).
/// Crie um asset via: botão direito no Project > Create > Metroidvania > Player Stats.
/// Ajuste o "game feel" no Inspector sem tocar no código.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Metroidvania/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movimento horizontal")]
    public float runSpeed = 7f;
    public float groundAccel = 80f;
    public float groundDecel = 90f;
    public float airAccel = 55f;
    public float airDecel = 50f;

    [Header("Pulo")]
    public float jumpHeight = 3.2f;          // altura máxima (unidades)
    public float jumpTimeToPeak = 0.36f;     // tempo até o topo (s)
    public float jumpTimeToDescent = 0.30f;  // tempo de queda (s)
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f; // corte ao soltar
    public float maxFallSpeed = 22f;

    [Header("Assistências (game feel)")]
    public float coyoteTime = 0.10f;         // pular logo após sair da borda
    public float jumpBufferTime = 0.12f;     // registrar pulo antes de aterrissar

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.45f;
    public int airDashes = 1;

    [Header("Combate")]
    public int maxHealth = 5;
    public int attackDamage = 1;
    public float invulnTime = 1.0f;
    public float knockbackForce = 12f;

    // --- Valores derivados: gravidade calculada a partir da altura/tempo do pulo ---
    public float JumpVelocity  => (2f * jumpHeight) / jumpTimeToPeak;
    public float JumpGravity   => (2f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak);
    public float FallGravity   => (2f * jumpHeight) / (jumpTimeToDescent * jumpTimeToDescent);
}
