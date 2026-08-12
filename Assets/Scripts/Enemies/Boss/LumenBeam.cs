using UnityEngine;

/// <summary>
/// Feixe de memória disparado pelo Guardião. Anda reto, some ao bater no
/// cenário ou depois de um tempo - projétil não pode ficar vivo para sempre.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LumenBeam : MonoBehaviour
{
    public int dano = 1;
    public float tempoDeVida = 5f;
    public LayerMask paredes;

    Vector2 direcao = Vector2.right;
    float velocidade = 7f;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Lancar(Vector2 dir, float vel)
    {
        direcao = dir.normalized;
        velocidade = vel;
        // aponta o sprite na direção do voo
        float ang = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }

    void Update()
    {
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);

        tempoDeVida -= Time.deltaTime;
        if (tempoDeVida <= 0f) { Destroy(gameObject); return; }

        // bateu na parede
        if (Physics2D.OverlapCircle(transform.position, 0.12f, paredes))
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        p.TakeDamage(dano, transform.position);
        Destroy(gameObject);
    }
}
