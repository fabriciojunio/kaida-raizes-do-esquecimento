using UnityEngine;

/// <summary>
/// Qualquer coisa que a espada de Kaida possa ferir. Existe para o ataque do
/// jogador não precisar conhecer cada tipo de inimigo - inimigo comum e chefe
/// respondem pela mesma porta.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 sourcePos);
}
