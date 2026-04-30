using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI bossNameText;
    public Slider healthSlider;

    private TreeBossAI currentBoss;
    public string bossDisplayName = "Tree Guardian";

    public void ShowBossUI(TreeBossAI boss)
    {
        currentBoss = boss;
        gameObject.SetActive(true);

        // 이름 표시
        if (bossNameText != null)
            bossNameText.text = bossDisplayName;

        // 초기 체력값 세팅
        if (healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.currentHealth;
        }

        Debug.Log($"[BossHealthUI] 보스 UI 표시: {bossDisplayName}");
    }

    void Update()
    {
        if (currentBoss == null) return;

        // 실시간 체력 갱신
        healthSlider.value = currentBoss.currentHealth;

        // 사망 시 자동 숨김
        if (currentBoss.currentHealth <= 0)
        {
            HideUI();
        }
    }

    public void HideUI()
    {
        currentBoss = null;
        gameObject.SetActive(false);
    }
}
