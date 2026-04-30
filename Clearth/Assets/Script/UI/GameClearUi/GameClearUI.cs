using UnityEngine;
using TMPro;

public class GameClearUI : MonoBehaviour
{
    private float inactiveTimeCounter; // 비활성화 상태에서 경과한 시간
    private int monsterKillCount; // 비활성화 상태에서 처치한 몬스터 수

    public TextMeshProUGUI minuteText;
    public TextMeshProUGUI secondText;
    public TextMeshProUGUI killCountText;

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(inactiveTimeCounter / 60f);
        int seconds = Mathf.FloorToInt(inactiveTimeCounter % 60f);

        minuteText.text = minutes.ToString("D2");
        secondText.text = seconds.ToString("D2");
    }

    // 외부에서 비활성화 시간 누적을 위한 메서드
    public void AddInactiveTime(float deltaTime)
    {
        inactiveTimeCounter += deltaTime;
        UpdateUI();
    }

    public void OnMonsterDestroyed()
    {
        if (!gameObject.activeInHierarchy)
        {
            monsterKillCount++;
            UpdateUI();
        }
    }

    public int GetInactiveTime() => (int)inactiveTimeCounter;
}
