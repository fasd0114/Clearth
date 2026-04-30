using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBossAI : MonoBehaviour
{
    [Header("Stat")]
    public int maxHealth = 300;
    public int currentHealth;
    public bool isInvincible = false;

    [Header("Pattern")]
    public float patternInterval = 4f;   // 패턴 텀
    private bool isCastingPattern = false;
    private bool inSpecialPhase = false;
    private bool isDead = false;

    [Header("체력 구분")]
    private bool phase2Triggered = false; // 66% 이하
    private bool phase3Triggered = false; // 33% 이하

    [Header("Reference")]
    public Animator anim;
    public Transform player;

    [Header("Pattern-Root")]
    public Transform[] rootPoints;            
    public GameObject rootWarningPrefab;      // 반투명 빨간 이펙트
    public GameObject rootAttackPrefab;       // 실제 뿌리 공격 오브젝트
    public float rootWarningTime = 1.0f;      // 경고 표시 시간
    public float rootAttackLifeTime = 1.5f;   // 뿌리 오브젝트 유지 시간
    public int rootSpawnCount = 3;            

    [Header("Spawn Range")]
    public Transform spawnMinPoint;
    public Transform spawnMaxPoint;
    public float minSpawnX = -8f;     // fallback
    public float maxSpawnX = 8f;
    public float playerSpawnRadius = 3f;      

    [Header("Pattern-Seed")]
    public GameObject seedPrefab;
    public int seedCount = 8;
    public float seedSpawnInterval = 0.3f;    // 씨앗 스폰 간격

    [Header("Special Pattern-Laser")]
    public GameObject laserObject;           // 레이저
    public float laserDuration = 3f;
    public ParticleSystem startVFX;

    [Header("Death Settings")]
    public GameObject spawnPrefab;
    public Transform spawnPoint;

    private bool cancelCurrentPattern = false;

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        StartCoroutine(PatternLoop());
    }

    // 패턴 루프
    private IEnumerator PatternLoop()
    {
        while (!isDead)
        {
            if (!isCastingPattern && !inSpecialPhase)
            {
                // 시작 전 cancel 플래그 초기화
                cancelCurrentPattern = false;

                int rand = Random.Range(0, 2);
                if (rand == 0)
                    yield return StartCoroutine(Pattern1_RootAttack());
                else
                    yield return StartCoroutine(Pattern2_SeedRain());

                // 패턴 간 인터벌 (인터럽트 가능)
                float timer = 0f;
                while (timer < patternInterval && !inSpecialPhase && !isDead)
                {
                    if (cancelCurrentPattern) break;
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                // 특수 패턴 중이거나 캐스팅 중이면 한 프레임 대기
                yield return null;
            }
        }
    }

    // 바닥 뿌리
    private IEnumerator Pattern1_RootAttack()
    {
        isCastingPattern = true;
        if (anim != null)
            anim.SetTrigger("Attack1");

        int attackCount = Mathf.Min(rootSpawnCount, 5);

        for (int i = 0; i < attackCount; i++)
        {
            if (cancelCurrentPattern || inSpecialPhase || isDead)
                break;

            float minX = spawnMinPoint ? spawnMinPoint.position.x : minSpawnX;
            float maxX = spawnMaxPoint ? spawnMaxPoint.position.x : maxSpawnX;

            float playerX = player ? player.position.x : 0f;
            float allowedMin = Mathf.Max(minX, playerX - playerSpawnRadius);
            float allowedMax = Mathf.Min(maxX, playerX + playerSpawnRadius);
            if (allowedMin > allowedMax)
            {
                allowedMin = minX;
                allowedMax = maxX;
            }

            float randX = Random.Range(allowedMin, allowedMax);
            float spawnY = (rootPoints != null && rootPoints.Length > 0)
                ? rootPoints[Random.Range(0, rootPoints.Length)].position.y
                : transform.position.y;

            Vector3 spawnPos = new Vector3(randX, spawnY, 0f);

            GameObject warningInstance = null;
            if (rootWarningPrefab)
                warningInstance = Instantiate(rootWarningPrefab, spawnPos, Quaternion.identity);

            float waited = 0f;
            while (waited < rootWarningTime)
            {
                if (cancelCurrentPattern || inSpecialPhase || isDead) break;
                waited += Time.deltaTime;
                yield return null;
            }

            if (cancelCurrentPattern || inSpecialPhase || isDead)
            {
                if (warningInstance) Destroy(warningInstance);
                break;
            }

            if (warningInstance) Destroy(warningInstance);

            Managers.Sound.Play("TreeAttack", 0.2f);

            GameObject rootInstance = Instantiate(rootAttackPrefab, spawnPos, Quaternion.identity);
            Destroy(rootInstance, rootAttackLifeTime);

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.5f);
        isCastingPattern = false;
    }

    // 씨앗 낙하
    private IEnumerator Pattern2_SeedRain()
    {
        isCastingPattern = true;

        if (anim != null)
            anim.SetTrigger("Attack2");

        Camera cam = Camera.main;
        if (cam == null)
        {
            isCastingPattern = false;
            yield break;
        }

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float spawnY = camPos.y + (camHeight / 2f) + 1.5f;

        for (int i = 0; i < seedCount; i++)
        {
            if (cancelCurrentPattern || inSpecialPhase || isDead)
                break;

            float minX = spawnMinPoint != null ? spawnMinPoint.position.x : minSpawnX;
            float maxX = spawnMaxPoint != null ? spawnMaxPoint.position.x : maxSpawnX;
            float randX = Random.Range(minX, maxX);

            Vector3 spawnPos = new Vector3(randX, spawnY, 0f);

            Instantiate(seedPrefab, spawnPos, Quaternion.identity);

            // interval 동안 인터럽트 가능 대기
            float waited = 0f;
            while (waited < seedSpawnInterval)
            {
                if (cancelCurrentPattern || inSpecialPhase || isDead)
                    break;
                waited += Time.deltaTime;
                yield return null;
            }

            if (cancelCurrentPattern || inSpecialPhase || isDead)
                break;
        }

        // 패턴 끝 여유 시간
        if (!cancelCurrentPattern && !inSpecialPhase && !isDead)
        {
            float tailWait = 0.5f;
            float t = 0f;
            while (t < tailWait)
            {
                if (cancelCurrentPattern || inSpecialPhase || isDead) break;
                t += Time.deltaTime;
                yield return null;
            }
        }

        isCastingPattern = false;
    }

    // 피격 처리
    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        if (anim != null)
            anim.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            KillBoss();
            return;
        }

        // 체력 비율에 따라 특수 패턴 진입
        float ratio = (float)currentHealth / maxHealth;

        if (!phase2Triggered && ratio <= 0.66f)
        {
            phase2Triggered = true;
            StartSpecialPhaseImmediate();
        }
        else if (!phase3Triggered && ratio <= 0.33f)
        {
            phase3Triggered = true;
            StartSpecialPhaseImmediate();
        }
    }

    private void StartSpecialPhaseImmediate()
    {
        cancelCurrentPattern = true;

        isCastingPattern = false;

        StartCoroutine(SpecialPhase_Laser());
    }

    // 특수 패턴
    private IEnumerator SpecialPhase_Laser()
    {
        inSpecialPhase = true;
        isInvincible = true;

        laserEnded = false;

        if (startVFX != null)
        {
            startVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            startVFX.Play();
        }

        if (anim != null)
            anim.SetTrigger("LaserStart");

        yield return new WaitUntil(() => laserEnded || isDead);

        if (startVFX != null)
        {
            startVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (anim != null)
            anim.SetTrigger("Groggy");

        // 특수 패턴 종료 처리
        isInvincible = false;
        inSpecialPhase = false;

        cancelCurrentPattern = false;
    }

    private bool laserEnded = false;

    public void FireLaser()
    {
        Managers.Sound.Play("BossLaser", 0.4f);

        if (laserObject != null)
        {
            laserObject.SetActive(true);
            var laser = laserObject.GetComponent<LaserController>();
            if (laser != null)
                StartCoroutine(LaserRoutine(laser));
        }
    }

    private IEnumerator LaserRoutine(LaserController laser)
    {
        laserEnded = false;
        laser.Activate();
        float t = 0f;
        while (t < laserDuration && !isDead)
        {
            t += Time.deltaTime;
            yield return null;
        }

        laser.Deactivate();
        laserEnded = true;
    }

    // 사망 처리
    public void KillBoss()
    {
        if (isDead) return;
        isDead = true;
        isInvincible = true;

        StopAllCoroutines();

        if (laserObject != null)
            laserObject.SetActive(false);

        GameObject[] roots = GameObject.FindGameObjectsWithTag("RootAttack");
        foreach (var r in roots) Destroy(r);

        GameObject[] seeds = GameObject.FindGameObjectsWithTag("BossSeed");
        foreach (var s in seeds) Destroy(s);

        GameObject[] rootWarnings = GameObject.FindGameObjectsWithTag("RootWarning");
        foreach (var w in rootWarnings) Destroy(w);

        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        // 사망 애니메이션 출력
        if (anim != null)
            anim.SetTrigger("Die");

        yield return new WaitForSeconds(1f);

        // 사망 프리팹 생성
        if (spawnPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject purification = Instantiate(spawnPrefab, spawnPos, Quaternion.identity);

            PurificationObject purifyScript = purification.GetComponent<PurificationObject>();
            if (purifyScript != null)
                purifyScript.SetBossReference(this);
        }
    }
    public void RemoveBossAfterPurify()
    {
        Destroy(gameObject);
    }
}
