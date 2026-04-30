using UnityEngine;
using TMPro;  // TextMeshPro 네임스페이스 추가

public class PlayTimeUI : MonoBehaviour
{
    [SerializeField] public TMP_Text playTimeText;  // TMP_Text로 변경

    private float playTime = 0f;  // 경과 시간을 저장하는 변수

    void Update()
    {
        // 플레이 시간을 계속 누적
        playTime += Time.deltaTime;

        // 시간 형식 변경 (분:초 형식)
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);

        // UI 텍스트 갱신
        playTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
