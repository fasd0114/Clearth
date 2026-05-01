using UnityEngine;
using UnityEngine.UI;

public class LoadingTime : MonoBehaviour
{
    public Slider slider; // 슬라이더 UI를 연결
    private float speed = 0.25f; // 슬라이더의 이동 속도 설정

    void Update()
    {
        // 슬라이더가 오른쪽으로 일정한 속도로 이동
        slider.value += speed * Time.deltaTime;

        // 슬라이더가 최대값에 도달하면 0으로 초기화
        if (slider.value >= slider.maxValue)
        {
            slider.value = slider.minValue;
        }
    }
}
