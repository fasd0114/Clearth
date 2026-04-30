using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DestroyEvent))]
[RequireComponent(typeof(MonsterDestroy))]

public class BossMonster1 : MonoBehaviour
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
    private float maxKnockbackDistance = 0.2f;  // 최대 이동 거리
    public float knockbackDistance = 2f;  // 정해진 넉백 거리
    private Vector3 knockbackStartPos;  // 넉백이 시작된 위치
    Rigidbody2D rb;
    private Animator animator;

    // 랜덤이동 관련로직
    private Vector2 randomDirection; // 랜덤 이동 방향
    public float changeDirectionTime = 3f; // 랜덤방향 이동 변경주기
    private float timer = 0; // 랜덤이동 타이머
    private enum MonsterState { Idle, Chasing }; // 상태 관리
    private MonsterState currentState = MonsterState.Idle;

    //보스 패턴 변수
    public GameObject trapPrefab;  // 트랩 Prefab
    public float trapLaunchForce = 10f;  // 트랩을 발사하는 힘
    private bool isAttacking = false;
    private float attackCooldown = 8f;  // 공격 후 대기 시간
    private float lastAttackTime = 0f;  // 마지막 공격 시간

    public int trapCount = 6;  // 발사할 트랩의 개수
    private float trapXSpacing = 8f;  // 트랩 간의 X 간격

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
        player = GameObject.FindWithTag("Player").transform;
        MonsterMovement();
        Vector3 fixedPosition = transform.position;
        fixedPosition.z = 0f;  // Z값 고정
        transform.position = fixedPosition;

        // isRunning 애니메이션 상태를 자동으로 설정
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0f);

        // Health가 0 이하일 때 "IsDying" 애니메이션을 트리거
        if (health <= 0 && !animator.GetBool("IsDying"))
        {
            // 죽는 애니메이션 실행
            animator.SetBool("IsDying", true);
            StartCoroutine(DieAfterDelay(1f)); // 3초 후에 몬스터 삭제
        }

        // 공격이 끝났고, 마지막 공격 시간부터 8초가 지나면 공격 시작
        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            // 랜덤 공격 패턴 실행
            int attackPattern = Random.Range(1, 3);  // 1 또는 2를 랜덤으로 선택
            if (attackPattern == 1)
            {
                AttackPattern1();  // 첫 번째 공격 패턴
            }
            else if (attackPattern == 2)
            {
                AttackPattern2();  // 두 번째 공격 패턴
            }
        }
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
        if (isAttacking) return;  // 공격 중일 때는 추격을 멈추도록
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y); // 수평 이동

        // Flip the monster to face the player based on movement direction
        if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(10f, 10f, 1f); // Facing left
        }
        else if (rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(-10f, 10f, 1f); // Facing right
        }
    }

    void RandomMovement()
    {
        if (isAttacking) return;  // 공격 중일 때는 랜덤 이동을 멈추도록

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
            transform.localScale = new Vector3(10f, 10f, 1f); // 왼쪽으로 이동
        }
        else if (randomDirection.x > 0)
        {
            transform.localScale = new Vector3(-10f, 10f, 1f); // 오른쪽으로 이동
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
                animator.SetBool("IsDying", true); // 죽는 애니메이션 실행
                StartCoroutine(DieAfterDelay(1f)); // 3초 뒤 몬스터 삭제
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
            knockbackStartPos = transform.position;

            Vector2 knockbackDirection = hitDirection.normalized * knockbackForce;
            rb.AddForce(knockbackDirection, ForceMode2D.Impulse);

            StartCoroutine(CheckKnockbackDistance(rb));
        }
    }

    private IEnumerator CheckKnockbackDistance(Rigidbody2D rb)
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
    }

    void StopKnockback(Rigidbody2D rb)
    {
        rb.velocity = Vector2.zero;
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

    // 첫 번째 공격 패턴
    void AttackPattern1()
    {
        if (isAttacking) return;  // 이미 공격 중이면 다시 실행되지 않도록

        isAttacking = true;
        animator.SetTrigger("IsAttack1");  // "IsAttack" 트리거로 애니메이션 실행

        // 이동을 멈춤
        rb.velocity = Vector2.zero;
        // 공격 후 대기
        StartCoroutine(AttackCooldown());

        // 트랩 발사
        LaunchTraps();
    }

    // 트랩 발사 로직
    void LaunchTraps()
    {
        // 트랩을 일정 간격으로 발사
        for (int i = 0; i < trapCount; i++)
        {
            // 발사 위치 계산: y는 -20으로 고정하고, x는 보스의 x 위치에서 일정 간격만큼 떨어짐
            float spawnX = transform.position.x + (i - (trapCount / 2)) * trapXSpacing;  // 가운데를 기준으로 왼쪽, 오른쪽으로 일정 간격
            Vector3 trapSpawnPosition = new Vector3(spawnX, transform.position.y - 20f, transform.position.z); // y값은 -20으로 고정

            // 트랩을 발사할 위치에서 트랩을 생성
            GameObject trap = Instantiate(trapPrefab, trapSpawnPosition, Quaternion.identity);

            // 트랩의 Rigidbody2D에 AddForce를 적용하여 수직으로 발사
            Rigidbody2D rb = trap.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;  // 기존 속도를 초기화
                rb.AddForce(Vector2.up * trapLaunchForce, ForceMode2D.Impulse);  // 위쪽으로 힘을 가하여 발사
            }

            // 트랩에 "Trap" 태그가 있어야 하므로 확인
            if (trap.CompareTag("Trap"))
            {
                Debug.Log($"트랩 {i + 1} 발사됨!");
            }
        }
    }

    // 두 번째 공격 패턴 (다른 형태의 공격을 추가할 수 있습니다)
    void AttackPattern2()
    {
        if (isAttacking) return;  // 이미 공격 중이면 다시 실행되지 않도록
        // 공격 중 y 값을 낮추기
        Vector3 newPosition = transform.position;
        newPosition.y -= 4f; // 예시로 y값을 2만큼 낮춤
        isAttacking = true;
        animator.SetTrigger("IsAttack");  // "IsAttack" 트리거로 애니메이션 실행

        
        transform.position = newPosition;

        // 이동을 멈춤
        rb.velocity = Vector2.zero;



        // 예시: 트랩 대신 다른 공격 패턴을 추가 (예: 레이저, 범위 공격 등)
        // 다른 공격 패턴을 여기에 구현

        // 공격 후 대기
        StartCoroutine(AttackCooldown());

    }


    // 애니메이션이 끝날 때까지 기다리는 코루틴
    private IEnumerator WaitForAnimationToEnd()
    {
        // 애니메이션이 끝날 때까지 기다립니다.
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length;

        yield return new WaitForSeconds(animationDuration);  // 애니메이션 길이만큼 대기

        // 공격 후 대기 시간이 끝나면 이동을 재개
        StartCoroutine(AttackCooldown());
    }

    // 공격 쿨타임 관리
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1f);  // 공격 후 대기 시간
        lastAttackTime = Time.time;  // 마지막 공격 시간 갱신
        isAttacking = false;  // 공격 끝
        // 이동을 재개
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }


}