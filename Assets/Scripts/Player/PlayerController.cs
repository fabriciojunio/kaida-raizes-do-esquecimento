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

    [Header("Detecção de parede (habilidade wall_climb)")]
    public Transform wallCheck;            // um filho vazio na altura do tronco
    public float wallCheckDistance = 0.28f;

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
    [HideInInspector] public int airJumpsLeft = 0;    // pulo duplo (habilidade double_jump)

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
        // Trabalha sempre sobre uma cópia. O asset PlayerStats é compartilhado
        // e fica no disco: sem clonar, a dificuldade escolhida e cada Nódulo
        // de Vida pego ficariam gravados no projeto e vazariam para a próxima
        // partida (e para o repositório).
        stats = stats == null
            ? ScriptableObject.CreateInstance<PlayerStats>()
            : Instantiate(stats);

        GameSettings.Aplicar(stats);
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
        Machine.Add("wallcling", new PlayerWallClingState(this));
    }

    /// <summary>
    /// Consulta uma habilidade desbloqueada. Se não existe SaveSystem na cena
    /// (ex.: cena de teste isolada), considera tudo liberado para não travar.
    /// </summary>
    public bool HasAbility(string id)
    {
        if (SaveSystem.Instance == null) return true;
        return SaveSystem.Instance.HasAbility(id);
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
        wallJumpLockTimer = Mathf.Max(0f, wallJumpLockTimer - dt);
        if (dashCooldownTimer == 0f) canDash = true;
    }

    // ---------- utilidades usadas pelos estados ----------
    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public float InputX => Input.GetAxisRaw("Horizontal");

    /// <summary>
    /// Posição de um marcador espelhada para o lado que a Kaida encara.
    ///
    /// Virar de lado troca só o flipX do sprite: o objeto em si não gira, e os
    /// marcadores presos a ele continuam parados à direita. Sem espelhar aqui,
    /// tudo que depende de "à frente" só funciona virado para a direita - o
    /// golpe saía pelas costas e a parede da esquerda nunca era detectada.
    /// </summary>
    public Vector2 PontoAFrente(Transform marcador)
    {
        var local = marcador.localPosition;
        return transform.TransformPoint(new Vector3(local.x * facing, local.y, local.z));
    }

    /// <summary>Parede à frente na direção que o jogador encara.</summary>
    public bool IsTouchingWall() => TemParedeNoLado(facing);

    /// <summary>
    /// Parede de verdade num dos lados: precisa responder na altura do tronco
    /// e mais embaixo.
    ///
    /// Um raio só transformava a lateral de qualquer plataforma de uma unidade
    /// em parede escalável, e a Kaida grudava no ar toda vez que passava
    /// raspando por uma delas. As duas alturas ficam a quase uma unidade de
    /// distância, então só superfície alta responde nas duas.
    /// </summary>
    bool TemParedeNoLado(int lado)
    {
        if (wallCheck == null || lado == 0) return false;

        var local = wallCheck.localPosition;
        return Sonda(local.x * lado, local.y, lado)
            && Sonda(local.x * lado, local.y - 0.9f, lado);
    }

    bool Sonda(float x, float y, int lado)
    {
        Vector3 origem = transform.TransformPoint(new Vector3(x, y, 0f));
        return Physics2D.Raycast(origem, Vector2.right * lado, wallCheckDistance, groundLayer);
    }

    /// <summary>
    /// Lado onde existe parede ao alcance: +1 à direita, -1 à esquerda, 0 nenhuma.
    ///
    /// Olha para os dois lados, e não só para onde a Kaida encara. Quem cai
    /// dentro de um poço encosta na parede antes de conseguir acertar a
    /// direção, e exigir a tecla certa no frame exato do toque era o que fazia
    /// a escalada parecer que não respondia.
    /// </summary>
    public int LadoDaParede()
    {
        if (TemParedeNoLado(facing)) return facing;       // o lado encarado tem preferência
        if (TemParedeNoLado(-facing)) return -facing;
        return 0;
    }

    /// <summary>
    /// Condição de agarrar na parede: habilidade, estar no ar e ter parede ao
    /// lado. Basta não estar empurrando para longe dela - soltar o controle
    /// gruda, que é o comportamento que o jogador espera.
    /// </summary>
    public bool CanWallCling()
    {
        if (!HasAbility("wall_climb") || IsGrounded()) return false;

        // logo após saltar de uma parede ela ainda está encostada nela: sem
        // esta trava o salto era cancelado no frame seguinte e a Kaida
        // simplesmente escorregava de volta
        if (wallJumpLockTimer > 0f) return false;

        int lado = LadoDaParede();
        if (lado == 0) return false;
        return !(Mathf.Abs(InputX) > 0.01f && Mathf.Sign(InputX) != lado);
    }

    /// <summary>Pulo no ar disponível (habilidade double_jump).</summary>
    public bool CanAirJump() => HasAbility("double_jump") && airJumpsLeft > 0;

    /// <summary>
    /// Marca que o próximo "jump" é um pulo aéreo. O PlayerJumpState lê a flag
    /// no Enter para aplicar a força reduzida em vez da força de pulo do chão.
    /// </summary>
    [HideInInspector] public bool pendingAirJump = false;
    [HideInInspector] public float wallJumpLockTimer = 0f;

    public void ConsumeAirJump()
    {
        airJumpsLeft--;
        pendingAirJump = true;
    }

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
    public void RefreshAirAbilities()
    {
        airDashesLeft = stats.airDashes;
        airJumpsLeft = HasAbility("double_jump") ? stats.airJumps : 0;
    }

    // ---------- combate ----------
    public void DoAttackHit()
    {
        if (attackPoint == null) return;
        // Área retangular, não circular: o golpe precisa alcançar tanto o
        // javali rente ao chão quanto a abelha pairando acima da cabeça. Um
        // círculo com raio suficiente para o alto avançaria longe demais.
        var area = new Vector2(attackRadius * 2f, attackRadius * 2.6f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(PontoAFrente(attackPoint), area, 0f, enemyLayer);
        // um inimigo pode ter vários colliders: só conta um golpe por alvo
        var atingidos = new System.Collections.Generic.HashSet<object>();
        foreach (var h in hits)
        {
            var alvo = h.GetComponentInParent<IDamageable>();
            if (alvo == null || atingidos.Contains(alvo)) continue;
            atingidos.Add(alvo);
            alvo.TakeDamage(stats.attackDamage, transform.position);
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

    /// <summary>
    /// Liga a invulnerabilidade e agenda o desligamento. O estado Hurt chama isso
    /// ao sair do knockback, para o jogador ter uma janela de recuperação.
    /// </summary>
    public void StartInvulnWindow()
    {
        if (invulnCoroutine != null) StopCoroutine(invulnCoroutine);
        isInvulnerable = true;
        invulnCoroutine = StartCoroutine(InvulnWindowRoutine());
    }
    Coroutine invulnCoroutine;
    System.Collections.IEnumerator InvulnWindowRoutine()
    {
        // pisca o sprite enquanto dura a janela, para o jogador ler o estado
        float elapsed = 0f;
        while (elapsed < stats.invulnTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.08f);
            elapsed += 0.08f;
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvulnerable = false;
        invulnCoroutine = null;
    }

    /// <summary>Cancela a janela de invulnerabilidade (usado no respawn).</summary>
    public void CancelInvulnWindow()
    {
        if (invulnCoroutine != null) { StopCoroutine(invulnCoroutine); invulnCoroutine = null; }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    /// <summary>Reemite o evento de vida (HUD reconstrói após respawn/troca de cena).</summary>
    public void NotifyHealthChanged() => HealthChanged?.Invoke(health, stats.maxHealth);

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
            Gizmos.DrawWireCube(PontoAFrente(attackPoint),
                new Vector3(attackRadius * 2f, attackRadius * 2.6f, 0f));
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 origem = PontoAFrente(wallCheck);
            Gizmos.DrawLine(origem, origem + Vector3.right * facing * wallCheckDistance);
        }
    }
}
