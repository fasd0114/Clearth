using UnityEngine;
using System.Collections;

public class BossCutsceneManager : MonoBehaviour
{
    [Header("연출 관련")]
    public GameObject blackBars;
    private BlackBarController barController;

    [Header("플레이어 관련")]
    public Transform player;
    public Transform playerTargetPosition;
    public float playerMoveSpeed = 10f;
    private Animator playerAnim;
    private PlayerController playerController;

    [Header("보스 관련")]
    public Animator bossAnimator;
    public BossController bossController;

    private bool isCutsceneRunning = false;

    void Start()
    {
        if (blackBars != null)
            barController = blackBars.GetComponent<BlackBarController>();
    }

    public void StartCutscene()
    {
        if (!isCutsceneRunning)
            StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        isCutsceneRunning = true;
        yield return StartCoroutine(WaitForPlayer());

        playerAnim = player.GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // 플레이어 제어권 상실 및 물리 멈춤
        if (playerController != null)
            playerController.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (playerAnim != null)
        {
            playerAnim.SetBool("isRunning", false);
            playerAnim.SetFloat("yVelocity", 0f);
            playerAnim.Play("Idle");
        }

        // 연출 시작
        if (barController != null)
            yield return StartCoroutine(barController.ShowBars());

        // 보스 상태를 Awake로 변경
        if (bossController != null)
        {
            bossController.ChangeState(BossController.BossState.Awake);
    }

        // 플레이어 후퇴 연출 시작
        if (playerAnim != null)
            playerAnim.SetTrigger("Cutscene");

        if (player != null && playerTargetPosition != null)
        {
            while (Vector2.Distance(player.position, playerTargetPosition.position) > 0.05f)
            {
                player.position = Vector2.MoveTowards(
                    player.position,
                    playerTargetPosition.position,
                    playerMoveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 플레이어 도착 및 연출 종료 대기
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        playerAnim.ResetTrigger("Cutscene");
        playerAnim.SetTrigger("CutsceneArrive");
        playerAnim.SetBool("isRunning", false);

        yield return new WaitUntil(() =>
            playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Cutscene_Arrive") &&
            playerAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f
        );
        playerAnim.Play("Idle");

        // 연출 마무리 단계
        yield return new WaitForSeconds(2.5f);

        if (barController != null)
            yield return StartCoroutine(barController.HideBars());

        // 보스 상태를 Battle로 변경
        if (bossController != null)
        {
            bossController.ChangeState(BossController.BossState.Battle);
    }

        // 플레이어 제어권 복구
        if (rb != null)
            rb.isKinematic = false;

        if (playerController != null)
            playerController.enabled = true;

        isCutsceneRunning = false;
    }

    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                player = found.transform;
                break;
            }
            yield return null;
        }
    }
}
