using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(DestroyEvent))]
[RequireComponent(typeof(MonsterDestroy))]
public class NewMonsterS : MonoBehaviour
{
    public MonsterDataSO monsterDataSO;

    public int health;
    private int attack;
    private float range;
    private float attackRange;
    private float speed;
    private int id;
    public Transform player;
    private Collider2D attackCollider;

    // 넉백 관련
    private float knockbackForce = 8f;
    private float maxKnockbackDuration = 0.5f;
    public float knockbackDistance = 2f;
    private Vector3 knockbackStartPos;
    Rigidbody2D rb;
    private Animator animator;
    private MonsterState prevState;
    private SpriteRenderer spriteRenderer;

    // 랜덤 이동 관련
    private Vector2 randomDirection;
    public float changeDirectionTime = 3f;
    private float timer = 0;

    // 상태 정의
    private enum MonsterState { Idle, Chasing, Attack, Dying, Knockback };
    private MonsterState currentState = MonsterState.Idle;

    private float separationRadius = 1f;
    private float separationForce = 0.3f;

    // 💥 공격 쿨타임 관련
    private bool canAttack = true;
    public float attackCooldown = 2f; // 공격 후 재공격 대기 시간 (초)
    private Coroutine flashCoroutine; // 현재 깜빡임 코루틴 저장용

    void Awake()
    {
        float jitter = 0.1f;
        Vector2 randomOffset = Random.insideUnitCircle * jitter;
        transform.position += (Vector3)randomOffset;
    }

    void FixedUpdate()
    {
        if (currentState == MonsterState.Dying) return;
        if (currentState == MonsterState.Knockback) return;
        ApplySeparation();
    }

    void Start()
    {
        string monsterName = gameObject.name.Replace("(Clone)", "").Trim();
        MonsterDataSO data = Managers.Data.GetMonsterDataByName(monsterName);

        if (data != null)
            AssignData(data);
        else
            Debug.LogError($"Monster 이름을 파싱할 수 없습니다: {monsterName}");

        MonsterCollisionIgnore();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>(); // 💡 여기서 초기화

        attackCollider = transform.Find("MonsterAttack")?.GetComponent<Collider2D>();
        if (attackCollider != null)
            attackCollider.enabled = false; // 기본은 꺼둠
        else
            Debug.LogWarning($"{gameObject.name} ▶ 'MonsterAttack' 오브젝트를 찾을 수 없습니다.");

    }

    void AssignData(MonsterDataSO data)
    {
        monsterDataSO = data;
        health = data.health;
        attack = data.attack;
        attackRange = data.attackRange;
        id = data.id;
        range = data.range;
        speed = data.speed;

        Debug.Log($"몬스터 데이터 적용됨: {data.monsterName} (ID: {data.id})");
    }

    void Update()
    {
        // 💥 죽거나 넉백 중이거나 공격 중일 때는 상태 갱신 중단
        if (currentState == MonsterState.Dying) return;
        if (currentState == MonsterState.Knockback) return;
        if (currentState == MonsterState.Attack) return; // 공격 중엔 이동 정지

        player = GameObject.FindWithTag("Player").transform;
        HandleState(); // 상태 기반 업데이트 호출
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0f);
    }

    void HandleState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case MonsterState.Idle:
                if (distanceToPlayer < range)
                    currentState = MonsterState.Chasing;
                RandomMovement();
                break;

            case MonsterState.Chasing:
                if (distanceToPlayer > range * 1.5f) // 💥 플레이어와 너무 멀어지면 추격 중단
                {
                    ChangeState(MonsterState.Idle);
                    Debug.Log($"{gameObject.name} ▶ 플레이어와 너무 멀어짐 → 랜덤 이동으로 복귀");
                }
                else if (distanceToPlayer <= attackRange)
                {
                    ChangeState(MonsterState.Attack);
                }
                else
                {
                    ChasePlayer();
                }
                break;

            case MonsterState.Attack:
                HandleAttack(distanceToPlayer);
                break;
        }
    }

    void HandleAttack(float distanceToPlayer)
    {
        // 쿨타임 중이면 공격 불가
        if (!canAttack) return;

        rb.velocity = Vector2.zero; // 이동 정지

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Attack");
        }

        // 💥 공격 쿨타임 시작
        canAttack = false;
        StartCoroutine(AttackCooldown());

        // 공격 상태로 전환
        currentState = MonsterState.Attack;
    }

    // 💥 공격 종료 시 (Animation Event에서 호출)
    public void EndAttack()
    {
        if (currentState == MonsterState.Attack)
        {
            ChangeState(MonsterState.Chasing);
            Debug.Log($"{gameObject.name} ▶ 공격 종료 → 추격 상태 복귀");
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        Debug.Log($"{gameObject.name} ▶ 공격 가능 상태 복귀");
    }

    void ChangeState(MonsterState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case MonsterState.Attack:
                rb.velocity = Vector2.zero;
                animator.SetBool("isRunning", false);
                animator.SetTrigger("Attack");
                break;

            case MonsterState.Dying:
                rb.velocity = Vector2.zero;
                animator.SetBool("IsDying", true);
                StartCoroutine(DieAfterDelay(0.8f)); // 💥 수정: 3초 뒤 삭제
                break;

            case MonsterState.Chasing:
                animator.ResetTrigger("Attack");
                animator.SetBool("isRunning", true);
                break;
        }
    }

    void RandomMovement()
    {
        timer += Time.deltaTime;
        if (timer > changeDirectionTime)
        {
            randomDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
            timer = 0f;
        }
        rb.velocity = new Vector2(randomDirection.x * speed, rb.velocity.y);
        transform.localScale = randomDirection.x < 0
            ? new Vector3(5f, 5f, 1f)
            : new Vector3(-5f, 5f, 1f);
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        if (rb.velocity.x < 0)
            transform.localScale = new Vector3(5f, 5f, 1f);
        else if (rb.velocity.x > 0)
            transform.localScale = new Vector3(-5f, 5f, 1f);
    }


    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        health -= damage;

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashDamage());
        }

        if (health <= 0)
        {
            if (!animator.GetBool("IsDying"))
                ChangeState(MonsterState.Dying);
        }
        else
        {
            Knockback(hitDirection);
        }
    }

    private IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void Knockback(Vector3 hitDirection)
    {
        if (currentState == MonsterState.Knockback)
            return; // 이미 넉백 중이면 무시

        if (rb != null)
        {
            prevState = currentState;
            currentState = MonsterState.Knockback;
            rb.velocity = Vector2.zero;
            Vector2 knockDir = hitDirection.normalized * knockbackForce;
            rb.AddForce(knockDir, ForceMode2D.Impulse);
            StartCoroutine(EndKnockbackAfterTime(rb));
        }
    }

    private IEnumerator EndKnockbackAfterTime(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(maxKnockbackDuration);
        StopKnockback(rb);
    }

    void StopKnockback(Rigidbody2D rb)
    {
        rb.velocity = Vector2.zero;
        currentState = prevState;
    }

    void ApplySeparation()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 repulsion = Vector2.zero;
        int count = 0;

        foreach (var col in hits)
        {
            if (col.CompareTag("Monsters") && col.gameObject != gameObject)
            {
                Vector2 diff = (Vector2)(transform.position - col.transform.position);
                float dist = diff.magnitude;
                if (dist > 0f)
                {
                    repulsion += diff.normalized / dist;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            repulsion /= count;
            repulsion.y = 0f;
            rb.AddForce(repulsion * separationForce, ForceMode2D.Impulse);
        }
    }

    void MonsterCollisionIgnore()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monsters");

        for (int i = 0; i < monsters.Length; i++)
        {
            for (int j = i + 1; j < monsters.Length; j++)
            {
                Collider col1 = monsters[i].GetComponent<Collider>();
                Collider col2 = monsters[j].GetComponent<Collider>();

                if (col1 != null && col2 != null)
                    Physics.IgnoreCollision(col1, col2);
            }
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = Color.white; // 💡 기본값으로 white 강제
        spriteRenderer.color = originalColor;

        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        int flashCount = 3;
        float flashDuration = 0.1f;

        for (int i = 0; i < flashCount; i++)
        {
            // 투명화
            spriteRenderer.color = transparentColor;
            yield return new WaitForSeconds(flashDuration);

            // 원상 복구
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // 💡 깜빡임 종료 시 강제 복구 (이 부분이 매우 중요!)
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    // 💥 애니메이션 이벤트에서 호출할 함수
public void EnableAttackCollider()
{
    if (attackCollider != null)
    {
        attackCollider.enabled = true;
        Debug.Log($"{gameObject.name} ▶ 공격 콜라이더 활성화");
            string monsterName = gameObject.name.Replace("(Clone)", "").Trim();

            if (monsterName.Contains("WoodSlime"))
            {
                Managers.Sound.Play("TreeAttack",0.2f);
            }
            else if (monsterName.Contains("Plant"))
            {
                Managers.Sound.Play("FlowerAttack",0.5f);
            }
            else if (monsterName.Contains("Golem"))
            {
                Managers.Sound.Play("GolemAttack",0.5f);
            }
        }
}

// 💥 애니메이션 이벤트에서 호출할 함수
public void DisableAttackCollider()
{
    if (attackCollider != null)
    {
        attackCollider.enabled = false;
        Debug.Log($"{gameObject.name} ▶ 공격 콜라이더 비활성화");
    }
}

}
