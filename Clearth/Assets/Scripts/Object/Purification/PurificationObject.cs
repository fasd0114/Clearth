using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PurificationObject : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactRange = 10f;
    public SpriteRenderer spriteRenderer;   // 오브젝트 본체
    public SpriteRenderer fKeyIcon;         // F 아이콘 (자식 SpriteRenderer)
    public GameObject outlineObject;        // 테두리 오브젝트
    //public string ForestClearRoom = "ForestClearRoom"; // 타이틀 씬 이름 (필요시 수정)

    private Transform player;
    private bool isNear = false;
    private bool isInteracting = false;

    private TreeBossAI bossReference;
    public void SetBossReference(TreeBossAI boss)
    {
        bossReference = boss;
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterSpawn());
    }

    private IEnumerator InitializeAfterSpawn()
    {
        // 플레이어 생성 대기
        while (GameObject.FindGameObjectWithTag("Player") == null)
            yield return null;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        // F키 아이콘, 아웃라인 기본 숨김
        if (fKeyIcon != null)
            fKeyIcon.gameObject.SetActive(false);

        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

    private void Update()
    {
        if (isInteracting || player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);

        // 접근 / 이탈 판정
        if (distance <= interactRange && !isNear)
        {
            isNear = true;

            // 테두리만 켜기
            if (outlineObject != null)
                outlineObject.SetActive(true);

            if (fKeyIcon != null)
                fKeyIcon.gameObject.SetActive(true);
        }
        else if (distance > interactRange && isNear)
        {
            isNear = false;

            // 테두리만 끄기
            if (outlineObject != null)
                outlineObject.SetActive(false);

            if (fKeyIcon != null)
                fKeyIcon.gameObject.SetActive(false);
        }

        // F키 입력
        if (isNear && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(StartPurifySequence());
        }
    }

    private IEnumerator StartPurifySequence()
    {
        isInteracting = true;

        if (fKeyIcon != null)
            fKeyIcon.gameObject.SetActive(false);

        if (outlineObject != null)
            outlineObject.SetActive(false);

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.fadeLock = false;

        Managers.Sound.Play("Purify", 0.2f);

        Managers.Sound.PlayBgm("EndRoomBGM", 0.2f);

        // 화면 페이드 인
        yield return FadeManager.Instance.FadeIn(3f);

        if (bossReference != null)
            bossReference.RemoveBossAfterPurify();

        // 정화 후 이동 or 전환
        Vector3 moveOffset = new Vector3(158.9f, 0f, 0f);
        player.position += moveOffset;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position += moveOffset;
        }
        //SceneManager.LoadScene(ForestClearRoom);  // 타이틀 씬으로 전환

        // 화면 페이드 아웃
        yield return FadeManager.Instance.FadeOut(3f);

        if (controller != null)
            controller.fadeLock = true;

        isInteracting = false;
    }
}
