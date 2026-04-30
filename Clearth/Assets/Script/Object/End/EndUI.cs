using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndUI : MonoBehaviour
{
    public static EndUI Instance;

    [Header("UI Components")]
    public GameObject panel;        
    public Image backgroundImage;   
    public TMP_Text messageText;   
    public Button actionButton;     

    [Header("UI Settings")]
    [TextArea]
    public string endMessage;
    public string buttonLabel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnButtonClick);
            Debug.Log("버튼 리스너 등록 완료");
        }

        if (messageText != null)
            messageText.text = endMessage;

        if (actionButton != null)
        {
            TMP_Text btnText = actionButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = buttonLabel;
        }
    }
    public void OpenEndUI()
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnButtonClick()
    {
        // 게임 멈춤 해제
        Time.timeScale = 1f;

        // 디버그용 로그 (눌렸는지 확인)
        Debug.Log("엔드 UI 버튼 클릭됨, 타이틀씬으로 이동 시도");

        // 씬 전환
        SceneManager.LoadScene("TitleScene");
    }
}
