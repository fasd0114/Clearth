using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashCooldown = 1f;

    bool isDashing;
    float dashCooldownEnd;

    [Header("Jump")]
    public float jumpForce = 2.3f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibleDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("Death Zone")]
    public float deathYThreshold = -100f;  // 플레이어가 사망하는 Y 좌표 값 (이 값보다 아래로 내려가면 팝업 실행)

    [Header("Edge Grab")]
    public Transform edgeCheck; // 머리 부분에 부착된 감지 포인트
    public float edgeCheckRadius = 0.1f;
    private Transform currentEdgePoint;
    bool isClimbingEdge;              // EdgeGrab 애니메이션 중인지 여부

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public LayerMask wallLayer;

    [Header("Attack Hitbox")]
    public AttackHitbox attackHitbox;
    bool IsFacingWall()
    {
        if (wallCheck == null) return false;

        Collider2D col = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        if (col != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Header("Wall Slide and Jump")]
    public float wallSlideSpeed = 1.0f; // 벽에서 떨어지는 속도
    public float wallJumpForce = 15.0f;  // 벽 점프 힘
    public Vector2 wallJumpDirection = new Vector2(1f, 1.5f);
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    private bool isTouchingWall = false;
    void CheckWall() { isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer); }

    [Header("Vine Jump Lock")]
    [HideInInspector] public bool isPostVineLocked = false;
    public void LockMovement() { isPostVineLocked = true; }

    //덩쿨 감지
    [HideInInspector] public bool isHangingFromVine = false;

    int attackIndex = 0;
    bool comboPossible = false;
    bool comboQueued = false;
    float comboTimer = 0f;
    float comboWindow = 0.8f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;

    public bool isGrounded;
    public bool isAttacking;
    public bool isJumpAttacking;
    bool isInvincible;
    bool isDead;
    public bool fadeLock = true;

    int monsterLayer;

    //트랩에 데미지를 입는데 필요한 변수
    private float trapDamageCooldown = 0.2f;  // 트랩에서 데미지를 입히는 시간 간격
    private float trapDamageTimer = 1f;  // 트랩 데미지 타이머
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public static PlayerController Instance { get; private set; }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        monsterLayer = LayerMask.NameToLayer("Monster");
        if (monsterLayer < 0)
            Debug.LogWarning("Monster 없음");
        else
            Physics2D.IgnoreLayerCollision(gameObject.layer, monsterLayer, true);

        if (groundCheck == null)
        {
            var t = transform.Find("GroundCheck");
            if (t != null) groundCheck = t;
            else Debug.LogWarning("GroundCheck 없음");
        }
    }

    void Start()
    {
        rb.freezeRotation = true;
        currentHealth = maxHealth;
        Debug.Log($"[Player] Current Health: {currentHealth}");
    }

    void Update()
    {
        if (!fadeLock) return;
        if (isDead || isClimbingEdge || isHangingFromVine) return;
        // 플레이어의 y좌표가 deathYThreshold 이하로 내려갔을 경우
        if (transform.position.y <= deathYThreshold)
        {
            Die();  // 사망 처리
        }
        CheckGround();
        CheckWall();
        if (isPostVineLocked)
        {
            if (!isGrounded) return;
            isPostVineLocked = false;
            anim.Play("Idle");
        }
        if (!isDashing)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= dashCooldownEnd)
            {
                // 현재 상태(Attack1/2/JumpAtk 등)를 모두 캔슬
                if (isAttacking || isJumpAttacking)
                    CancelAttack();

                Dash();
                // 대쉬 시전 프레임에서는 다른 처리 하지 않음
                UpdateAnimParams();
                ResetAttackFlags();
                return;
            }
        }
        if (isAttacking)
        {
            HandleAttack();     // 콤보 입력 처리
            UpdateAnimParams();
            ResetAttackFlags();
            return;
        }
        if (!isDashing)
        {
            HandleMovement();
            HandleJump();
            HandleAttack();
            HandleWallJump();
            HandleWallSlide();
        }

        UpdateAnimParams();
        //ResetAttackFlags();
    }
    void LateUpdate()
    {
        ResetAttackFlags();
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleMovement()
    {
        // 공격 중이면 이동 불가
        if (isAttacking) return;

        float h = Input.GetAxisRaw("Horizontal");

        if (isWallJumping && Mathf.Abs(h) < 0.1f)
            return;

        // 입력이 들어오면 즉시 조작 가능
        if (isWallJumping && Mathf.Abs(h) >= 0.1f)
            isWallJumping = false;

        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        if (h != 0f)
        {
            transform.localScale = new Vector3(Mathf.Sign(h) * 7, 7, 1);
        }
    }
    void HandleJump()
    {
        // 공격 중이면 점프 불가
        if (isAttacking) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Managers.Sound.Play("Jump");
            anim.ResetTrigger("attack1");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }
    void HandleAttack()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (!isGrounded)
        {
            if (isJumpAttacking) return;

            var state = anim.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName("JumpAtk"))
            {
                anim.ResetTrigger("attack1");
                anim.ResetTrigger("attack2");
                anim.SetTrigger("jumpattack");
                isJumpAttacking = true;

            }
            return;
        }

        // 콤보 입력 처리
        if (!isAttacking)
        {
            // 1타 시작
            attackIndex = 1;
            anim.ResetTrigger("attack2");
            anim.SetTrigger("attack1");
            isAttacking = true;
            rb.velocity = new Vector2(0, rb.velocity.y);

            StartCoroutine(AttackRoutine());
        }
        else if (comboPossible)
        {
            comboQueued = true;
        }
    }
    IEnumerator AttackRoutine()
    {
        comboPossible = true;
        comboQueued = false;
        comboTimer = 0f;

        while (comboTimer < comboWindow)
        {
            comboTimer += Time.deltaTime;
            if (comboQueued)
            {
                comboQueued = false;
                comboPossible = false;
                attackIndex = 2;

                anim.ResetTrigger("attack1");
                anim.SetTrigger("attack2");

                yield return new WaitForSeconds(0.1f);
                break;
            }
            yield return null;
        }

        yield return new WaitUntil(() =>
            anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") == false &&
            anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2") == false
        );

        attackIndex = 0;
        isAttacking = false;           
        comboPossible = false;
        comboQueued = false;
    }

    void HandleWallSlide()
    {
        if (isWallJumping) return;

        float h = Input.GetAxisRaw("Horizontal");

        if (isTouchingWall && !isGrounded)
        {
            if (rb.velocity.y > 0.1f)
            {
                if (isWallSliding)
                {
                    isWallSliding = false;
                    anim.SetBool("isWallSliding", false);
                }
                return;
            }

            if (!isWallSliding)
            {
                isWallSliding = true;
                anim.SetBool("isWallSliding", true);
            }

            if (rb.velocity.y < -wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
        else
        {
            if (isWallSliding)
            {
                isWallSliding = false;
                anim.SetBool("isWallSliding", false);
            }
        }
    }
    void HandleWallJump()
    {
        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            isWallJumping = true;
            isWallSliding = false;
            anim.SetBool("isWallSliding", false);
            anim.SetTrigger("wallJump");

            int jumpDir = -Mathf.RoundToInt(transform.localScale.x);
            float h = Input.GetAxisRaw("Horizontal");

            if (jumpDir > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            else if (jumpDir < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            rb.velocity = Vector2.zero;
            Vector2 jumpForce = new Vector2(wallJumpDirection.x * jumpDir, wallJumpDirection.y).normalized * wallJumpForce;
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }
        if (isWallJumping && isGrounded)
            isWallJumping = false;
    }

    void UpdateAnimParams()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0f);
    }

    void ResetAttackFlags()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (isJumpAttacking)
        {
            if (state.IsName("JumpAtk"))
                return;
            else if (isGrounded)
            {
                StopCoroutine(nameof(ResetJumpAttackAfterLanding));
                StartCoroutine(ResetJumpAttackAfterLanding());
            }
        }
    }
    IEnumerator ResetJumpAttackAfterLanding()
    {
        yield return new WaitForSeconds(0.03f);
        isJumpAttacking = false;
    }
    public void EndAttack()
    {
        isAttacking = false;
    }

    public void EndJumpAttack()
    {
        isJumpAttacking = false;
    }

    public void TryDamageFrom(Collider2D other)
    {
        if (isInvincible) return;

        // 'Monster' 레이어에서 데미지를 받는 로직
        if (other.gameObject.layer == monsterLayer)
        {
            TakeDamage(1);
        }

        // 'Trap' 태그를 가진 오브젝트에서 데미지를 받는 로직
        if (other.CompareTag("Trap"))
        {
            // trapDamageCooldown 동안에만 데미지를 주도록 설정
            trapDamageTimer += Time.deltaTime;
            if (trapDamageTimer >= trapDamageCooldown)
            {
                TakeDamage(2);
                // trap에 부딪히면 데미지 2
                trapDamageTimer = 0f;  // 타이머 초기화
            }
        }
    }


    public void TakeDamage(int dmg)
    {
        if (isInvincible) return;
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        Debug.Log($"[Player] Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
    }
    private void Die()
    {
        isDead = true;
        Debug.Log("[Player] 사망");
        Managers.Popup.OpenGameOver();
    }


    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibleDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
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
        var dashClip = anim.runtimeAnimatorController.animationClips
            .First(clip => clip.name == "Dash");
        float speed = 2f;
        float dashAnimLength = dashClip.length / speed;

        dashCooldownEnd = Time.time + dashCooldown;
        anim.SetBool("isDashing", true);
        isInvincible = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = Mathf.Sign(transform.localScale.x);
        float dashVelocity = dashDistance / dashAnimLength;

        // y위치 기억
        float fixedY = transform.position.y;

        float timer = 0f;
        while (timer < dashAnimLength)
        {
            rb.velocity = new Vector2(direction * dashVelocity, 0f);

            //  y좌표 고정
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        anim.SetBool("isDashing", false);
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        isInvincible = false;
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EdgeGrabPoint") && !isClimbingEdge && IsFacingWall())
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

        float direction = Mathf.Sign(transform.localScale.x);

        Vector3 targetCameraPos = edgePoint.position + new Vector3(direction * 0.4f, 2.05f, 0f);

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null)
            cam.StartEdgeGrabLerp(targetCameraPos);


        transform.position = edgePoint.position;
        currentEdgePoint = edgePoint;
        anim.SetTrigger("edgeGrab");

        yield return new WaitUntil(() => !isClimbingEdge);
    }
    public void OnEdgeGrabClimbEnd()
    {
        float direction = Mathf.Sign(transform.localScale.x);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 0f;

        Vector3 correctedPos = currentEdgePoint.position + new Vector3(direction * 0.4f, 2.05f, 0f);
        transform.position = correctedPos;

        rb.gravityScale = 2.5f;
        isClimbingEdge = false;

        if (isPostVineLocked)
            isPostVineLocked = false;

        anim.Play("Idle");
    }
    void OnDrawGizmosSelected()
    {
        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(edgeCheck.position, edgeCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
    public void PlayAttackSound()
    {
        Managers.Sound.Play("Attack", 0.5f);
    }
    public void Heal(int amount)
    {
        if (isDead) return;

        // 체력 회복, 최대 체력 제한
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    public void CancelAttack()
    {
        if (attackHitbox != null)
            attackHitbox.DisableHitbox();

        isAttacking = false;
        isJumpAttacking = false;
        comboPossible = false;
        comboQueued = false;
        attackIndex = 0;

        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("jumpattack");
    }
}
