using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance;

    [Header("UI References")]
    public GameObject panel;
    public Animator tutorialAnimator;
    public Image keyGuideImage;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public TextMeshProUGUI pageIndicator;

    [Header("Page Data")]
    [TextArea] public string[] tutorialTexts;
    public string[] animationTriggers;
    public Sprite[] keyGuideSprites;

    private int currentPage = 0;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        closeButton.onClick.AddListener(CloseTutorial);
    }

    public void OpenTutorial()
    {
        if (isOpen) return;

        isOpen = true;
        panel.SetActive(true);
        currentPage = 0;
        Time.timeScale = 0f;
        UpdatePage();
    }

    public void CloseTutorial()
    {
        isOpen = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void NextPage()
    {
        if (currentPage < animationTriggers.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        // 애니메이션 출력
        if (tutorialAnimator != null && animationTriggers.Length > 0)
        {
            // 모든 트리거 초기화 
            foreach (string trigger in animationTriggers)
            {
                tutorialAnimator.ResetTrigger(trigger);
            }

            // 현재 페이지 트리거만 활성화
            if (currentPage < animationTriggers.Length)
                tutorialAnimator.SetTrigger(animationTriggers[currentPage]);
        }

        // 키 가이드 이미지 출력
        if (keyGuideSprites.Length > 0 && currentPage < keyGuideSprites.Length)
        {
            keyGuideImage.sprite = keyGuideSprites[currentPage];
            keyGuideImage.enabled = keyGuideSprites[currentPage] != null;
        }

        // 설명 텍스트 갱신
        if (tutorialTexts.Length > 0 && currentPage < tutorialTexts.Length)
            tutorialText.text = tutorialTexts[currentPage];

        // 페이지 인디케이터
        if (pageIndicator != null)
            pageIndicator.text = $"{currentPage + 1} / {animationTriggers.Length}";

        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(currentPage < animationTriggers.Length - 1);
        closeButton.gameObject.SetActive(currentPage == animationTriggers.Length - 1);
    }
}