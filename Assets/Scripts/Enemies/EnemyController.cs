using UnityEngine;

/// <summary>
/// Inimigo base: patrulha, detecta o jogador, persegue e ataca por contato.
/// Serve de base para inimigos mais específicos (voadores, atiradores, etc.)
/// via herança ou composição — mantido simples de propósito.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int contactDamage = 1;
    public float moveSpeed = 2f;
    public float detectRange = 4f;
    public float attackRange = 0.8f;
    public float attackCooldown = 1f;

    [Header("Patrulha")]
    public Transform patrolPointA;
    public Transform patrolPointB;

    [Header("Detecção")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Animação (opcional)")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    Rigidbody2D rb;
    int health;
    int facing = 1;
    float attackTimer;
    Transform target;
    Vector3 patrolTarget;
    bool dying = false;

    public System.Action<EnemyController> Died;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
        if (patrolPointA != null) patrolTarget = patrolPointA.position;
    }

    void Update()
    {
        if (dying) return;
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);

        FindPlayer();
        if (target != null)
            ChaseAndAttack();
        else
            Patrol();
    }

    void FindPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);
        target = hit != null ? hit.transform : null;
    }

    void ChaseAndAttack()
    {
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        SetFacing(target.position.x > transform.position.x ? 1 : -1);

        if (dist <= attackRange)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (attackTimer <= 0f)
            {
                var pc = target.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(contactDamage, transform.position);
                attackTimer = attackCooldown;
            }
        }
        else
        {
            rb.velocity = new Vector2(facing * moveSpeed, rb.velocity.y);
        }
    }

    void Patrol()
    {
        if (patrolPointA == null || patrolPointB == null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        float distToTarget = Mathf.Abs(transform.position.x - patrolTarget.x);
        if (distToTarget < 0.1f)
            patrolTarget = (patrolTarget == (Vector3)patrolPointA.position) ? patrolPointB.position : patrolPointA.position;

        int dir = patrolTarget.x > transform.position.x ? 1 : -1;
        SetFacing(dir);
        rb.velocity = new Vector2(dir * moveSpeed * 0.6f, rb.velocity.y);
    }

    void SetFacing(int dir)
    {
        if (dir != facing)
        {
            facing = dir;
            if (spriteRenderer != null) spriteRenderer.flipX = (facing < 0);
        }
    }

    public void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (dying) return;
        health -= amount;
        if (animator != null) animator.Play("hurt");
        Vector2 kb = new Vector2(Mathf.Sign(transform.position.x - sourcePos.x) * 3f, 2f);
        rb.velocity = kb;
        if (health <= 0) Die();
    }

    void Die()
    {
        dying = true;
        if (animator != null) animator.Play("death");
        Died?.Invoke(this);
        Destroy(gameObject, 0.4f);
    }

    public int Health => health;
    public bool Dying => dying;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (patrolPointA != null && patrolPointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
        }
    }
}
