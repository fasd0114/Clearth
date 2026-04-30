using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;

    /// <summary>
    /// 체력바를 활성화
    /// </summary>
    public void EnableHealthBar()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 체력바를 비활성화
    /// </summary>
    public void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 0과 1 사이의 값을 백분율로 체력 막대 값을 설정
    /// </summary>
    public void SetHealthBarValue(float HPPercent)
    {
        healthBar.transform.localScale = new Vector3(HPPercent, 1f, 1f);
    }
}
