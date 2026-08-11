using UnityEngine;

/// <summary>
/// Perigo do cenário: espinhos da Floresta, poços de esporos da Caverna.
/// Causa dano por contato e, se configurado, devolve o jogador ao último
/// checkpoint em vez de deixar ele preso dentro do perigo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public int damage = 1;
    [Tooltip("Poço sem fundo: teleporta de volta ao checkpoint depois do dano.")]
    public bool returnToCheckpoint = false;
    public float repeatInterval = 0.8f;

    float cooldown;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) { TryHurt(other); }
    void OnTriggerStay2D(Collider2D other)  { TryHurt(other); }

    void TryHurt(Collider2D other)
    {
        if (cooldown > 0f) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;

        p.TakeDamage(damage, transform.position);
        cooldown = repeatInterval;

        if (returnToCheckpoint && GameManager.Instance != null && p.health > 0)
        {
            p.transform.position = GameManager.Instance.CurrentCheckpoint;
            p.SetVelocity(0f, 0f);
        }
    }
}
