using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


[RequireComponent(typeof(DestroyEvent))]
[RequireComponent(typeof(MonsterDestroy))]

public class Monster1 : MonoBehaviour
{

    public MonsterDataSO monsterDataSO;  // ScriptableObject로 데이터를 저장

    public int health;
    private int attack;
    private float range;

    private float attackRange;
    private float speed;
    private int id;  // 몬스터의 ID
    public Transform player;

    // 넉백 관련 로직
    private float knockbackForce = 8f;  // AddForce에 사용할 힘
    private float maxKnockbackDuration = 0.2f;  // 최대 이동 거리
    public float knockbackDistance = 2f;  // 정해진 넉백 거리
    private Vector3 knockbackStartPos;  // 넉백이 시작된 위치
    Rigidbody2D rb;
    private Animator animator;
    private MonsterState prevState;

    // 랜덤이동 관련로직
    private Vector2 randomDirection; // 랜덤 이동 방향
    public float changeDirectionTime = 3f; // 랜덤방향 이동 변경주기
    private float timer = 0; // 랜덤이동 타이머
    private enum MonsterState { Idle, Chasing, Dying, Knockback }; // 상태 관리
    private MonsterState currentState = MonsterState.Idle;

    private float separationRadius = 1f;
    private float separationForce = 0.3f;

    void Awake()
    {
        float jitter = 0.1f;
        Vector2 randomOffset = Random.insideUnitCircle * jitter;
        transform.position += (Vector3)randomOffset;
    }

    void FixedUpdate()
    {
        if (currentState == MonsterState.Dying) return;

        // 넉백 상태에서는 이동 로직을 완전히 무시
        if (currentState == MonsterState.Knockback) return;
        ApplySeparation();
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
            Debug.Log($"sep vector: {repulsion}, count: {count}");
            rb.AddForce(repulsion * separationForce, ForceMode2D.Impulse);
        }
    }

    void Start()
    {
        // "(Clone)"을 제거하고 이름을 가져옴
        string monsterName = gameObject.name.Replace("(Clone)", "").Trim();

        // 이름을 기준으로 몬스터 데이터를 검색
        MonsterDataSO data = Managers.Data.GetMonsterDataByName(monsterName);
        if (data != null)
        {
            AssignData(data);  // 데이터를 AI에 할당
        }
        else
        {
            Debug.LogError($"Monster 이름을 파싱할 수 없습니다: {monsterName}");
        }
        MonsterCollisionIgnore();

        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트
        rb.gravityScale = 1f; // 중력 적용 (바닥을 따라 이동)
        rb.freezeRotation = true;  // 회전을 고정하여 넘어지지 않게 만듦

        animator = GetComponent<Animator>(); // Animator 컴포넌트 참조
    }

    void AssignData(MonsterDataSO data)
    {
        monsterDataSO = data;

        health = monsterDataSO.health;
        attack = monsterDataSO.attack;
        attackRange = monsterDataSO.attackRange;
        id = monsterDataSO.id;
        range = monsterDataSO.range;
        speed = monsterDataSO.speed;

        Debug.Log($"몬스터 데이터 적용됨: {monsterDataSO.monsterName} (ID: {monsterDataSO.id})");
    }

    void Update()
    {
        if (currentState == MonsterState.Dying) return;

        // 넉백 상태에서는 이동 로직을 완전히 무시
        if (currentState == MonsterState.Knockback) return;

        player = GameObject.FindWithTag("Player").transform;
        MonsterMovement();
        Vector3 fixedPosition = transform.position;
        fixedPosition.z = 0f;  // Z값 고정
        transform.position = fixedPosition;

        // isRunning 애니메이션 상태를 자동으로 설정
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0f);

    }

    // 몬스터 사망 후 3초 지연 후 몬스터 제거
    private IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 3초 대기
        DestroyMonster(); // 몬스터 제거
    }

    private void DestroyMonster()
    {
        Destroy(gameObject);  // 게임 오브젝트 삭제
    }

    void MonsterMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < range)
        {
            currentState = MonsterState.Chasing;
        }
        else
        {
            currentState = MonsterState.Idle;
        }

        if (currentState == MonsterState.Chasing)
        {
            ChasePlayer();
        }
        else if (currentState == MonsterState.Idle)
        {
            RandomMovement();
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y); // 수평 이동

        // Flip the monster to face the player based on movement direction
        if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(5f, 5f, 1f); // Facing left
        }
        else if (rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(-5f, 5f, 1f); // Facing right
        }
    }

    void RandomMovement()
    {
        timer += Time.deltaTime;
        if (timer > changeDirectionTime)
        {
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            timer = 0f;
        }

        rb.velocity = new Vector2(randomDirection.x * speed, rb.velocity.y); // 랜덤 이동

        // Flip the monster based on movement direction
        if (randomDirection.x < 0)
        {
            transform.localScale = new Vector3(5f, 5f, 1f); // 왼쪽으로 이동
        }
        else if (randomDirection.x > 0)
        {
            transform.localScale = new Vector3(-5f, 5f, 1f); // 오른쪽으로 이동
        }
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} 가 {damage} 의 데미지를, remaining health: {health}");

        if (health <= 0)
        {
            // 죽는 애니메이션을 트리거하여 3초 후에 몬스터가 죽도록 처리
            if (!animator.GetBool("IsDying"))
            {
                animator.SetBool("IsDying", true);
                currentState = MonsterState.Dying;  // 죽는 애니메이션 실행
                StartCoroutine(DieAfterDelay(1.6f)); // 3초 뒤 몬스터 삭제
            }
        }
        else
        {
            Knockback(hitDirection);
        }
    }

    public void Knockback(Vector3 hitDirection)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            prevState = currentState;
            currentState = MonsterState.Knockback;
            //knockbackStartPos = transform.position;
            rb.velocity = Vector2.zero;
            Vector2 knockbackDirection = hitDirection.normalized * knockbackForce;
            rb.AddForce(knockbackDirection, ForceMode2D.Impulse);

            //StartCoroutine(CheckKnockbackDistance(rb));
            StartCoroutine(EndKnockbackAfterTime(rb));
        }
    }

    /*private IEnumerator CheckKnockbackDistance(Rigidbody2D rb)
    {
        while (true)
        {
            float distanceMoved = Vector3.Distance(knockbackStartPos, transform.position);

            if (distanceMoved >= maxKnockbackDistance)
            {
                StopKnockback(rb);
                yield break;
            }

            yield return null;
        }
    }*/

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

    // 몬스터가 사망할 때 아이템을 떨어트리는 로직(그냥 사망 로직과 통합)
    private void MonsterDestroyed()
    {
        DestroyEvent destroyedEvent = GetComponent<DestroyEvent>();
        destroyedEvent.CallDestroyedEvent(false, 1);
    }


    // 몬스터의 충돌 관련 로직
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Player player = other.GetComponent<Player>();
            if (player != null)
            {
                //player.TakeDamage(attack);
            }
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
                {
                    Physics.IgnoreCollision(col1, col2);
                }
            }
        }
    }
}