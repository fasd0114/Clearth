using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerHealth), typeof(EnvironmentChecker))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Move, Jump, Attack, Dash, WallSlide, WallJump, EdgeGrab, Dead }
    [Header("현재 상태")]
    public PlayerState currentState = PlayerState.Idle;

    [Header("이동 속도")]
    public float moveSpeed = 1.5f;

    [Header("대쉬 설정")]
    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    float dashCooldownEnd;

    [Header("점프력")]
    public float jumpForce = 2.3f;

    [Header("사망 Y좌표")]
    public float deathYThreshold = -100f;
    // 체력 값들은 PlayerHealth 컴포넌트에서 관리하지만 외부 참조를 위해 유지
    public int CurrentHealth => health != null ? health.CurrentHealth : 0;
    public int MaxHealth => health != null ? health.MaxHealth : 0;

    private float baseScale;

    [Header("Wall & Edge")]
    public float wallSlideSpeed = 1.0f;
    public float wallJumpForce = 15.0f;
    public Vector2 wallJumpDirection = new Vector2(1f, 1.5f);
    public float edgeCheckRadius = 0.1f;
    private bool isWallJumping = false;
    private Transform currentEdgePoint;

    [Header("전투 관련")]
    public AttackHitbox attackHitbox;
    private int attackIndex = 0;
    private bool comboPossible = false;
    private bool comboQueued = false;
    private float comboTimer = 0f;
    private float comboWindow = 0.8f;

    private float horizontalInput;
    private bool jumpRequested;
    private bool wallJumpRequested;

    // References
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;
    PlayerHealth health;
    EnvironmentChecker env;

    // Flags
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
        if (currentState == PlayerState.EdgeGrab || isHangingFromVine) return;

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
        if (CanAutoUpdateState())
        {
            UpdateAutoState();
        }
        UpdateAnimParams();
    }
    void FixedUpdate()
    {
        if (currentState == PlayerState.Dash || currentState == PlayerState.EdgeGrab || isHangingFromVine) return;

        if (isPostVineLocked)
        {
            if (!env.IsGrounded) return;
            isPostVineLocked = false;
            anim.Play("Idle");
        }

        HandleStatePhysics();

        if (jumpRequested)
        {
            ExecuteJump();
            jumpRequested = false;
        }

        if (wallJumpRequested)
        {
            ExecuteWallJump();
            wallJumpRequested = false;
        }
    }
    bool CanAutoUpdateState()
    {
        // 강제 상태일 때는 자동 전환을 막아 기존 동작이 끊기지 않게 함
        return currentState != PlayerState.Dash &&
               currentState != PlayerState.Attack &&
               currentState != PlayerState.WallJump &&
               currentState != PlayerState.EdgeGrab &&
               currentState != PlayerState.Dead;
    }
    void UpdateAutoState()
    {
        if (env.IsTouchingWall && !env.IsGrounded && rb.velocity.y <= 0.1f)
        {
            SetState(PlayerState.WallSlide);
        }
        else if (!env.IsGrounded)
        {
            SetState(PlayerState.Jump);
        }
        else if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            SetState(PlayerState.Move);
        }
        else
        {
            SetState(PlayerState.Idle);
        }
    }
    void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        // 이전 상태 탈출 시 처리
        switch (currentState)
        {
            case PlayerState.Dash:
                rb.gravityScale = 2.5f;
                anim.SetBool("isDashing", false);
                break;
            case PlayerState.WallSlide:
                anim.SetBool("isWallSliding", false);
                break;
            case PlayerState.EdgeGrab:
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.gravityScale = 2.5f;
                break;
        }
        currentState = newState;

        // 새로운 상태 진입 시 처리
        switch (currentState)
        {
            case PlayerState.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case PlayerState.Dash:
                anim.SetBool("isDashing", true);
                rb.gravityScale = 0f;
                break;
            case PlayerState.Jump:
                rb.gravityScale = 2.5f;
                break;
            case PlayerState.WallSlide:
                anim.SetBool("isWallSliding", true);
                break;
            case PlayerState.EdgeGrab:
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
            case PlayerState.Attack:
                if (env.IsGrounded) rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    void HandleStatePhysics()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
            case PlayerState.Jump:
            case PlayerState.Attack:
                ApplyMovement();
                break;
            case PlayerState.WallSlide:
                if (rb.velocity.y < -wallSlideSpeed) rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
                ApplyMovement();
                break;
            case PlayerState.WallJump:
                if (Mathf.Abs(horizontalInput) >= 0.1f) SetState(PlayerState.Jump);
                ApplyMovement();
                break;
        }
    }
    private void HandleInput()
    {
        if (currentState == PlayerState.Dead || currentState == PlayerState.EdgeGrab) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= dashCooldownEnd && currentState != PlayerState.Dash)
        {
            if (currentState == PlayerState.Attack) CancelAttack();
            Dash();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleAttack();
        }

        if (currentState != PlayerState.Dash && currentState != PlayerState.Attack)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (env.IsGrounded) jumpRequested = true;
                else if (env.IsTouchingWall) wallJumpRequested = true;
            }
        }
        else
        {
            horizontalInput = 0;
        }
    }
    void ApplyMovement()
    {
        if (isWallJumping && Mathf.Abs(horizontalInput) < 0.1f) return;
        if (isWallJumping && Mathf.Abs(horizontalInput) >= 0.1f) isWallJumping = false;

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        if (horizontalInput != 0f) transform.localScale = new Vector3(Mathf.Sign(horizontalInput) * baseScale, baseScale, 1f);
    }
    public void ResetMovementForVine()
    {
        horizontalInput = 0f;
        jumpRequested = false;
        wallJumpRequested = false;
        rb.velocity = Vector2.zero;

        // 애니메이션 파라미터 초기화 (추락 애니메이션 방지)
        anim.SetFloat("yVelocity", 0f);
        anim.SetBool("isRunning", false);
    }
    void ExecuteJump()
    {
        Managers.Sound.Play("Jump");
    anim.ResetTrigger("attack1"); 
    rb.velocity = new Vector2(rb.velocity.x, jumpForce); 
}

    void ExecuteWallJump()
    {
        isWallJumping = true;
        SetState(PlayerState.WallJump); // 즉시 상태 변경을 호출하여 Slide 애니메이션 해제[cite: 1]

        anim.SetTrigger("wallJump");

        int jumpDir = -Mathf.RoundToInt(transform.localScale.x / baseScale);
        transform.localScale = new Vector3(jumpDir * baseScale, baseScale, 1f);

        rb.velocity = Vector2.zero;
        Vector2 force = new Vector2(wallJumpDirection.x * jumpDir, wallJumpDirection.y).normalized * wallJumpForce;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    public void TakeDamage(int dmg) => health?.TakeDamage(dmg);
    public void Heal(int amount) => health?.Heal(amount);
    public void LockMovement() => isPostVineLocked = true;
    public void EndAttack()
    {
        if (currentState == PlayerState.Attack)
        {
            SetState(PlayerState.Idle);
    }
    }
    public void EndJumpAttack()
    {
        if (currentState == PlayerState.Attack)
        {
            SetState(PlayerState.Idle);
    }
    }
    public void PlayAttackSound() => Managers.Sound.Play("Attack", 0.5f);

    private void Die()
    {
        health?.TakeDamage(9999);
    }

    void HandleAttack()
    {
        // 이미 공격 중이라면 콤보 입력만 체크하고 리턴
        if (currentState == PlayerState.Attack)
        {
            if (comboPossible) comboQueued = true;
            return;
        }

        // 공중 공격 처리
        if (!env.IsGrounded)
        {
            SetState(PlayerState.Attack);
            anim.SetTrigger("jumpattack");
            return;
        }

        // 지상 공격 시작
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        // 시작 시 상태 변경
        SetState(PlayerState.Attack);

        attackIndex = 1;
        anim.SetTrigger("attack1");

        comboPossible = true; // 입력 허용
        comboQueued = false;
        comboTimer = 0f;

        while (comboTimer < comboWindow)
        {
            comboTimer += Time.deltaTime;
            if (comboQueued) // 재입력 시 2타 전이
            {
                attackIndex = 2;
                anim.SetTrigger("attack2");
                yield return new WaitForSeconds(0.1f);
                break;
            }
            yield return null;
        }

        // 애니메이션이 완전히 끝날 때까지 대기
        yield return new WaitUntil(() =>
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("JumpAtk"));

        comboPossible = false;
        comboQueued = false;
        attackIndex = 0;

        SetState(PlayerState.Idle);
    }

    public void Dash()
    {
        if (currentState == PlayerState.Dash) return;
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        SetState(PlayerState.Dash);
        Managers.Sound.Play("Dash");
        var dashClip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Dash");
        float dashAnimLength = dashClip.length / 2f;
        dashCooldownEnd = Time.time + dashCooldown;

        // 대시 중 높이 유지
        float direction = Mathf.Sign(transform.localScale.x);
        float dashVelocity = dashDistance / dashAnimLength;

        float timer = 0f;
        while (timer < dashAnimLength)
        {
            rb.velocity = new Vector2(direction * dashVelocity, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        SetState(PlayerState.Idle);
        rb.velocity = Vector2.zero;
    }

    // EdgeGrab 감지
    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState == PlayerState.EdgeGrab || currentState == PlayerState.Dead)
            return;

        float facingDir = Mathf.Sign(transform.localScale.x);
        if (other.CompareTag("EdgeGrabPoint") && env.IsFacingWall(facingDir))
        {
            StartCoroutine(EdgeGrabRoutine(other.transform));
        }
    }

    IEnumerator EdgeGrabRoutine(Transform edgePoint)
    {
        SetState(PlayerState.EdgeGrab);
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.StartEdgeGrabLerp(edgePoint.position + new Vector3(Mathf.Sign(transform.localScale.x) * 0.4f, 2.05f, 0f));

        transform.position = edgePoint.position;
        currentEdgePoint = edgePoint;
        anim.SetTrigger("edgeGrab");
        yield return new WaitUntil(() => currentState != PlayerState.EdgeGrab);
    }

    public void OnEdgeGrabClimbEnd()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = currentEdgePoint.position + new Vector3(direction * 0.4f, 2.05f, 0f);
        rb.gravityScale = 2.5f;
        SetState(PlayerState.Idle);
        anim.Play("Idle");
    }

    void UpdateAnimParams()
    {
        anim.SetBool("isGrounded", env.IsGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);
    }


    public void CancelAttack()
    {
        if (attackHitbox != null) attackHitbox.DisableHitbox();
        comboPossible = comboQueued = false;
        attackIndex = 0;

        anim.ResetTrigger("attack1"); 
        anim.ResetTrigger("attack2"); 
        anim.ResetTrigger("jumpattack");

        SetState(PlayerState.Idle);
    }
}