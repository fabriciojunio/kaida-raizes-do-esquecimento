using UnityEngine;

/// <summary>
/// Inimigo base: patrulha, detecta o jogador, persegue e ataca por contato.
/// Serve de base para inimigos mais específicos (voadores, atiradores, etc.)
/// via herança — os métodos de comportamento são virtuais de propósito.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour, IDamageable
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
    [Tooltip("Sem pontos de patrulha, anda até achar borda ou parede e volta.")]
    public float patrolDistance = 3f;

    [Header("Detecção")]
    public Transform groundCheck;   // à frente dos pés: detecta fim da plataforma
    public Transform wallCheck;     // à frente do tronco: detecta parede
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    [Tooltip("Se ligado, o inimigo só persegue quem ele realmente enxerga.")]
    public bool requireLineOfSight = true;

    [Tooltip("Altura dos olhos a partir do pivô, que fica nos pés.")]
    public float alturaDosOlhos = 0.7f;

    [Header("Animação (opcional)")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Tooltip("Marque se o desenho original olha para a esquerda. Os três " +
             "inimigos deste pacote olham; sem isso eles andam de costas.")]
    public bool spriteOlhaParaEsquerda = true;

    protected Rigidbody2D rb;
    protected int health;
    protected int facing = 1;
    protected float attackTimer;
    protected Transform target;
    protected bool dying = false;

    Vector3 patrolTarget;
    bool headingToB = true;
    Vector3 spawnPos;

    public System.Action<EnemyController> Died;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // a dificuldade mexe em quão rápido eles vêm e de quão longe percebem
        moveSpeed *= GameSettings.VelocidadeDosInimigos();
        detectRange *= GameSettings.AlcanceDeVisao();

        health = maxHealth;
        spawnPos = transform.position;
        patrolTarget = patrolPointB != null ? patrolPointB.position
                                            : spawnPos + Vector3.right * patrolDistance;
    }

    protected virtual void Update()
    {
        if (dying) return;
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);

        FerirQuemEncostar();

        FindPlayer();
        if (target != null) ChaseAndAttack();
        else Patrol();
    }

    /// <summary>
    /// Dano por encostar, medido pela sobreposição real dos colisores.
    ///
    /// Antes o dano dependia da distância entre os pivôs, que ficam nos pés:
    /// um javali colado no jogador podia registrar "longe" e não machucar
    /// ninguém. Encostou, dói.
    /// </summary>
    protected void FerirQuemEncostar()
    {
        if (attackTimer > 0f) return;

        var meuColisor = GetComponent<Collider2D>();
        if (meuColisor == null) return;

        var filtro = new ContactFilter2D();
        filtro.SetLayerMask(playerLayer);
        filtro.useTriggers = false;

        var encostados = new Collider2D[4];
        int quantos = meuColisor.OverlapCollider(filtro, encostados);

        for (int i = 0; i < quantos; i++)
        {
            var pc = encostados[i].GetComponentInParent<PlayerController>();
            if (pc == null) continue;

            pc.TakeDamage(contactDamage, transform.position);
            attackTimer = attackCooldown;
            return;
        }
    }

    protected virtual void FindPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);
        if (hit == null) { target = null; return; }

        if (requireLineOfSight)
        {
            // Na altura do peito, não dos pés. Os pivôs ficam na base dos
            // sprites, então um raio de pé a pé corre rente ao chão e bate no
            // próprio piso: o inimigo nunca enxergava o jogador e nunca
            // chegava a atacar.
            Vector2 origin = (Vector2)transform.position + Vector2.up * alturaDosOlhos;
            Vector2 alvo = (Vector2)hit.transform.position + Vector2.up * 1.4f;
            Vector2 dir = alvo - origin;

            var blocked = Physics2D.Raycast(origin, dir.normalized, dir.magnitude, groundLayer);
            if (blocked.collider != null) { target = null; return; }
        }
        target = hit.transform;
    }

    protected virtual void ChaseAndAttack()
    {
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        SetFacing(target.position.x > transform.position.x ? 1 : -1);

        if (dist <= attackRange)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            TryContactAttack();
        }
        else if (HasFloorAhead() && !HasWallAhead())
        {
            rb.velocity = new Vector2(facing * moveSpeed, rb.velocity.y);
        }
        else
        {
            // não persegue para dentro de um buraco
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    protected void TryContactAttack()
    {
        if (attackTimer > 0f) return;
        var pc = target != null ? target.GetComponent<PlayerController>() : null;
        if (pc != null) pc.TakeDamage(contactDamage, transform.position);
        attackTimer = attackCooldown;
    }

    protected virtual void Patrol()
    {
        if (patrolPointA != null && patrolPointB != null)
        {
            if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.15f)
            {
                headingToB = !headingToB;
                patrolTarget = headingToB ? patrolPointB.position : patrolPointA.position;
            }
        }
        else
        {
            // sem pontos definidos: vira ao chegar na borda da plataforma ou na parede
            if (!HasFloorAhead() || HasWallAhead()) SetFacing(-facing);
            rb.velocity = new Vector2(facing * moveSpeed * 0.6f, rb.velocity.y);
            PlayMoveAnim();
            return;
        }

        int dir = patrolTarget.x > transform.position.x ? 1 : -1;
        SetFacing(dir);
        if (!HasFloorAhead() || HasWallAhead())
        {
            // ponto de patrulha mal posicionado (do outro lado de um buraco)
            headingToB = !headingToB;
            patrolTarget = headingToB ? patrolPointB.position : patrolPointA.position;
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        rb.velocity = new Vector2(dir * moveSpeed * 0.6f, rb.velocity.y);
        PlayMoveAnim();
    }

    protected virtual void PlayMoveAnim()
    {
        if (animator != null && Mathf.Abs(rb.velocity.x) > 0.05f) animator.Play("walk");
    }

    /// <summary>Existe chão logo à frente? Impede o inimigo de andar para fora da plataforma.</summary>
    protected bool HasFloorAhead()
    {
        if (groundCheck == null) return true;
        return Physics2D.Raycast(groundCheck.position, Vector2.down, 0.6f, groundLayer);
    }

    protected bool HasWallAhead()
    {
        if (wallCheck == null) return false;
        return Physics2D.Raycast(wallCheck.position, Vector2.right * facing, 0.35f, groundLayer);
    }

    protected void SetFacing(int dir)
    {
        if (dir != 0 && dir != facing)
        {
            facing = dir;
            if (spriteRenderer != null)
                spriteRenderer.flipX = spriteOlhaParaEsquerda ? (facing > 0) : (facing < 0);
            RepositionChecks();
        }
    }

    /// <summary>Espelha os pontos de checagem junto com o sprite.</summary>
    void RepositionChecks()
    {
        if (groundCheck != null)
        {
            var lp = groundCheck.localPosition;
            groundCheck.localPosition = new Vector3(Mathf.Abs(lp.x) * facing, lp.y, lp.z);
        }
        if (wallCheck != null)
        {
            var lp = wallCheck.localPosition;
            wallCheck.localPosition = new Vector3(Mathf.Abs(lp.x) * facing, lp.y, lp.z);
        }
    }

    public virtual void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (dying) return;
        health -= amount;
        if (animator != null) animator.Play("hurt");
        float dir = Mathf.Sign(transform.position.x - sourcePos.x);
        if (dir == 0f) dir = -facing;
        rb.velocity = new Vector2(dir * 3f, 2f);
        if (health <= 0) Die();
        else StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;
        var original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.09f);
        if (spriteRenderer != null) spriteRenderer.color = original;
    }

    protected virtual void Die()
    {
        dying = true;
        rb.velocity = Vector2.zero;
        if (animator != null) animator.Play("death");
        // desliga a colisão para o cadáver não continuar empurrando o jogador
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        Died?.Invoke(this);
        Destroy(gameObject, 0.6f);
    }

    public int Health => health;
    public bool Dying => dying;
    public int Facing => facing;

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
