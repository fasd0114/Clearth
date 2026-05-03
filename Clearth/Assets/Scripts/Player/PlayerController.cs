using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerHealth), typeof(EnvironmentChecker))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Move, Jump, Attack, Dash, WallSlide, WallJump, EdgeGrab, Dead }
    [Header("Current State")]
    public PlayerState currentState = PlayerState.Idle;

    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    bool isDashing;
    float dashCooldownEnd;

    [Header("Jump")]
    public float jumpForce = 2.3f;

    [Header("Death & Health")]
    public float deathYThreshold = -100f;
    // 체력 값들은 PlayerHealth 컴포넌트에서 관리하지만, 외부 참조를 위해 유지합니다.
    public int CurrentHealth => health != null ? health.CurrentHealth : 0;
    public int MaxHealth => health != null ? health.MaxHealth : 0;

    private float baseScale;

    [Header("Wall & Edge")]
    public float wallSlideSpeed = 1.0f;
    public float wallJumpForce = 15.0f;
    public Vector2 wallJumpDirection = new Vector2(1f, 1.5f);
    public float edgeCheckRadius = 0.1f;
    private bool isWallJumping = false;
    private bool isClimbingEdge = false;
    private Transform currentEdgePoint;

    [Header("Combat & Hitbox")]
    public AttackHitbox attackHitbox;
    private int attackIndex = 0;
    private bool comboPossible = false;
    private bool comboQueued = false;
    private float comboTimer = 0f;
    private float comboWindow = 0.8f;

    // References
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;
    PlayerHealth health;
    EnvironmentChecker env;

    // Flags (기존 변수 모두 유지)
    public bool isAttacking;
    public bool isJumpAttacking;
    public bool fadeLock = true;
    [HideInInspector] public bool isPostVineLocked = false;
    [HideInInspector] public bool isHangingFromVine = false;

    public static PlayerController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
        env = GetComponent<EnvironmentChecker>();

        baseScale = Mathf.Abs(transform.localScale.x);

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Monster"), true);
    }

    void Update()
    {
        if (!fadeLock || (health != null && health.CurrentHealth <= 0)) return;
        if (isClimbingEdge || isHangingFromVine) return;

        // 사망 경계 체크
        if (transform.position.y <= deathYThreshold) Die();

        // 덩굴 이동 후 잠금 처리
        if (isPostVineLocked)
        {
            if (!env.IsGrounded) return;
            isPostVineLocked = false;
            anim.Play("Idle");
        }

        HandleInput();
        UpdateState();
        UpdateAnimParams();
    }

    void LateUpdate() { ResetAttackFlags(); }

    private void HandleInput()
    {
        // 1. Dash Input
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= dashCooldownEnd && !isDashing)
        {
            if (isAttacking || isJumpAttacking) CancelAttack();
            Dash();
            return;
        }

        // 2. Attack Input
        if (Input.GetMouseButtonDown(0)) HandleAttack();

        // 3. Move & Jump (공격 중이 아닐 때만)
        if (!isDashing && !isAttacking)
        {
            HandleMovement();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (env.IsGrounded) HandleJump();
                else if (env.IsTouchingWall) HandleWallJump();
            }
        }
    }

    // --- 기존 public 메서드 완벽 복구 ---[cite: 2]

    public void TakeDamage(int dmg) => health?.TakeDamage(dmg);
    public void Heal(int amount) => health?.Heal(amount);
    public void LockMovement() => isPostVineLocked = true;
    public void EndAttack() => isAttacking = false;
    public void EndJumpAttack() => isJumpAttacking = false;
    public void PlayAttackSound() => Managers.Sound.Play("Attack", 0.5f);

    private void Die()
    {
        // PlayerHealth의 Die는 팝업을 띄우고, Controller는 로직을 정지합니다.
        health?.TakeDamage(9999);
    }

    // --- 내부 로직 ---

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (isWallJumping && Mathf.Abs(h) < 0.1f) return;
        if (isWallJumping && Mathf.Abs(h) >= 0.1f) isWallJumping = false;

        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        if (h != 0f) transform.localScale = new Vector3(Mathf.Sign(h) * baseScale, baseScale, 1f);
    }

    void HandleJump()
    {
        Managers.Sound.Play("Jump");
        anim.ResetTrigger("attack1");
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void HandleWallJump()
    {
        isWallJumping = true;
        anim.SetTrigger("wallJump");

        int jumpDir = -Mathf.RoundToInt(transform.localScale.x / baseScale);

        // 다시 baseScaleX를 곱해 크기를 유지하며 방향만 반전시킵니다.[cite: 2]
        transform.localScale = new Vector3(jumpDir * baseScale, baseScale, 1f);

        rb.velocity = Vector2.zero;
        Vector2 force = new Vector2(wallJumpDirection.x * jumpDir, wallJumpDirection.y).normalized * wallJumpForce;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    void HandleAttack()
    {
        if (!env.IsGrounded)
        {
            if (isJumpAttacking) return;
            anim.SetTrigger("jumpattack");
            isJumpAttacking = true;
            return;
        }

        if (!isAttacking)
        {
            isAttacking = true;
            StartCoroutine(AttackRoutine());
        }
        else if (comboPossible) comboQueued = true;
    }

    IEnumerator AttackRoutine()
    {
        attackIndex = 1;
        anim.SetTrigger("attack1");
        rb.velocity = new Vector2(0, rb.velocity.y);
        comboPossible = true; comboQueued = false; comboTimer = 0f;
        while (comboTimer < comboWindow)
        {
            comboTimer += Time.deltaTime;
            if (comboQueued)
            {
                attackIndex = 2;
                anim.SetTrigger("attack2");
                yield return new WaitForSeconds(0.1f);
                break;
            }
            yield return null;
        }
        yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"));
        isAttacking = false; comboPossible = false; comboQueued = false; attackIndex = 0;
    }

    public void Dash()
    {
        if (isDashing) return;
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        Managers.Sound.Play("Dash");
        var dashClip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Dash");
        float dashAnimLength = dashClip.length / 2f;
        dashCooldownEnd = Time.time + dashCooldown;
        anim.SetBool("isDashing", true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float direction = Mathf.Sign(transform.localScale.x);
        float dashVelocity = dashDistance / dashAnimLength;
        float fixedY = transform.position.y;

        float timer = 0f;
        while (timer < dashAnimLength)
        {
            rb.velocity = new Vector2(direction * dashVelocity, 0f);
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        anim.SetBool("isDashing", false);
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;
    }

    // EdgeGrab 감지[cite: 2]
    void OnTriggerStay2D(Collider2D other)
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        if (other.CompareTag("EdgeGrabPoint") && !isClimbingEdge && env.IsFacingWall(facingDir))
        {
            StartCoroutine(EdgeGrabRoutine(other.transform));
        }
    }

    IEnumerator EdgeGrabRoutine(Transform edgePoint)
    {
        isClimbingEdge = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.StartEdgeGrabLerp(edgePoint.position + new Vector3(Mathf.Sign(transform.localScale.x) * 0.4f, 2.05f, 0f));

        transform.position = edgePoint.position;
        currentEdgePoint = edgePoint;
        anim.SetTrigger("edgeGrab");
        yield return new WaitUntil(() => !isClimbingEdge);
    }

    public void OnEdgeGrabClimbEnd()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = currentEdgePoint.position + new Vector3(direction * 0.4f, 2.05f, 0f);
        rb.gravityScale = 2.5f;
        isClimbingEdge = false;
        anim.Play("Idle");
    }

    void UpdateState()
    {
        if (isDashing) currentState = PlayerState.Dash;
        else if (isClimbingEdge) currentState = PlayerState.EdgeGrab;
        else if (isAttacking) currentState = PlayerState.Attack;
        else if (isWallJumping) currentState = PlayerState.WallJump;
        else if (env.IsTouchingWall && !env.IsGrounded && rb.velocity.y <= 0.1f)
        {
            currentState = PlayerState.WallSlide;
            if (rb.velocity.y < -wallSlideSpeed) rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            anim.SetBool("isWallSliding", true);
        }
        else
        {
            anim.SetBool("isWallSliding", false);
            if (!env.IsGrounded) currentState = PlayerState.Jump;
            else if (Mathf.Abs(rb.velocity.x) > 0.1f) currentState = PlayerState.Move;
            else currentState = PlayerState.Idle;
        }
    }

    void UpdateAnimParams()
    {
        anim.SetBool("isGrounded", env.IsGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);
    }

    void ResetAttackFlags()
    {
        if (isJumpAttacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("JumpAtk") && env.IsGrounded)
        {
            StartCoroutine(ResetJumpAttackAfterLanding());
        }
    }

    IEnumerator ResetJumpAttackAfterLanding()
    {
        yield return new WaitForSeconds(0.03f);
        isJumpAttacking = false;
    }

    public void CancelAttack()
    {
        if (attackHitbox != null) attackHitbox.DisableHitbox();
        isAttacking = isJumpAttacking = comboPossible = comboQueued = false;
        attackIndex = 0;
        anim.ResetTrigger("attack1"); anim.ResetTrigger("attack2"); anim.ResetTrigger("jumpattack");
    }
}