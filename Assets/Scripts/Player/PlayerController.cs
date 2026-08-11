using System;
using UnityEngine;

/// <summary>
/// Corpo do jogador (metroidvania 2D). Centraliza física (Rigidbody2D), vida,
/// timers de game feel e a máquina de estados. A lógica de cada ação fica nos
/// estados (pasta Player/States). Dirige um Animator opcional para tocar as
/// animações dos seus assets de arte.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public PlayerStats stats;

    [Header("Detecção de chão")]
    public Transform groundCheck;          // um filho vazio nos pés
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Combate")]
    public Transform attackPoint;          // ponto à frente do jogador
    public float attackRadius = 0.6f;
    public LayerMask enemyLayer;

    [Header("Animação (opcional)")]
    public Animator animator;              // arraste o Animator do seu sprite
    public SpriteRenderer spriteRenderer;

    // Eventos para HUD / GameManager
    public event Action<int,int> HealthChanged;
    public event Action Died;

    // Estado público (lido pelos estados)
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public int facing = 1;
    [HideInInspector] public int pendingKnockbackDir = 1;  // usado pelo estado Hurt
    [HideInInspector] public int health;
    [HideInInspector] public bool isInvulnerable = false;
    [HideInInspector] public bool canDash = true;
    [HideInInspector] public int airDashesLeft = 0;

    // timers
    [HideInInspector] public float coyoteTimer = 0f;
    [HideInInspector] public float jumpBufferTimer = 0f;
    [HideInInspector] public float dashCooldownTimer = 0f;

    public StateMachine Machine { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // gravidade controlada manualmente (game feel)
        rb.freezeRotation = true;
        if (stats == null) stats = ScriptableObject.CreateInstance<PlayerStats>();
        health = stats.maxHealth;

        BuildStateMachine();
    }

    void Start()
    {
        HealthChanged?.Invoke(health, stats.maxHealth);
        if (GameManager.Instance != null) GameManager.Instance.RegisterPlayer(this);
        Machine.SetInitial("idle");
    }

    void BuildStateMachine()
    {
        Machine = new StateMachine();
        Machine.Add("idle",   new PlayerIdleState(this));
        Machine.Add("run",    new PlayerRunState(this));
        Machine.Add("jump",   new PlayerJumpState(this));
        Machine.Add("fall",   new PlayerFallState(this));
        Machine.Add("dash",   new PlayerDashState(this));
        Machine.Add("attack", new PlayerAttackState(this));
        Machine.Add("hurt",   new PlayerHurtState(this));
        Machine.Add("dead",   new PlayerDeadState(this));
    }

    void Update()
    {
        TickTimers(Time.deltaTime);
        Machine.LogicUpdate();
    }

    void FixedUpdate()
    {
        Machine.PhysicsUpdate();
    }

    void TickTimers(float dt)
    {
        coyoteTimer = Mathf.Max(0f, coyoteTimer - dt);
        jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - dt);
        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - dt);
        if (dashCooldownTimer == 0f) canDash = true;
    }

    // ---------- utilidades usadas pelos estados ----------
    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public float InputX => Input.GetAxisRaw("Horizontal");

    public void ApplyGravity(float dt)
    {
        float g = (rb.velocity.y < 0f) ? stats.FallGravity : stats.JumpGravity;
        float vy = rb.velocity.y - g * dt;
        vy = Mathf.Max(vy, -stats.maxFallSpeed);
        rb.velocity = new Vector2(rb.velocity.x, vy);
    }

    public void ApplyHorizontal(float dt, float inputDir, float accel, float decel)
    {
        float targetX = inputDir * stats.runSpeed;
        float newX;
        if (Mathf.Abs(inputDir) > 0.01f)
        {
            newX = Mathf.MoveTowards(rb.velocity.x, targetX, accel * dt);
            SetFacing((int)Mathf.Sign(inputDir));
        }
        else
        {
            newX = Mathf.MoveTowards(rb.velocity.x, 0f, decel * dt);
        }
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    public void SetFacing(int dir)
    {
        if (dir != 0 && dir != facing)
        {
            facing = dir;
            if (spriteRenderer != null) spriteRenderer.flipX = (facing < 0);
        }
    }

    public void SetVelocity(float x, float y) { rb.velocity = new Vector2(x, y); }
    public void SetVelocityX(float x) { rb.velocity = new Vector2(x, rb.velocity.y); }
    public void SetVelocityY(float y) { rb.velocity = new Vector2(rb.velocity.x, y); }

    public void PlayAnim(string name)
    {
        if (animator != null) animator.Play(name);
    }

    public void BufferJump() { jumpBufferTimer = stats.jumpBufferTime; }
    public bool ConsumeJumpBuffer()
    {
        if (jumpBufferTimer > 0f) { jumpBufferTimer = 0f; return true; }
        return false;
    }
    public bool CanCoyoteJump() => coyoteTimer > 0f;
    public void RefreshAirAbilities() { airDashesLeft = stats.airDashes; }

    // ---------- combate ----------
    public void DoAttackHit()
    {
        if (attackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (var h in hits)
        {
            var e = h.GetComponentInParent<EnemyController>();
            if (e != null) e.TakeDamage(stats.attackDamage, transform.position);
        }
    }

    public void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (isInvulnerable || health <= 0) return;
        health = Mathf.Max(0, health - amount);
        HealthChanged?.Invoke(health, stats.maxHealth);
        if (health <= 0)
        {
            Died?.Invoke();
            Machine.ChangeState("dead");
        }
        else
        {
            int dir = (int)Mathf.Sign(transform.position.x - sourcePos.x);
            if (dir == 0) dir = -facing;
            pendingKnockbackDir = dir;
            Machine.ChangeState("hurt");
        }
    }

    public void StartInvulnWindow()
    {
        if (invulnCoroutine != null) StopCoroutine(invulnCoroutine);
        invulnCoroutine = StartCoroutine(InvulnWindowRoutine());
    }
    Coroutine invulnCoroutine;
    System.Collections.IEnumerator InvulnWindowRoutine()
    {
        yield return new WaitForSeconds(stats.invulnTime);
        isInvulnerable = false;
    }

    public void Heal(int amount)
    {
        health = Mathf.Min(stats.maxHealth, health + amount);
        HealthChanged?.Invoke(health, stats.maxHealth);
    }

    // debug/testes
    public string CurrentStateName => Machine != null ? Machine.CurrentName : "";

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
