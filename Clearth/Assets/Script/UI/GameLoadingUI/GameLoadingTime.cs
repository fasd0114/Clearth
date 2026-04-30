using UnityEngine;

using System.Collections; // 코루틴에 필요한 네임스페이스 추가
public class GameLoadingTime : MonoBehaviour
{
    public GameObject popup; // 팝업으로 사용할 UI 오브젝트

    void OnEnable()
    {
        // 팝업이 활성화되면 5초 뒤에 자동으로 꺼지게 설정
        StartCoroutine(ClosePopupAfterDelay(4f));
        Debug.Log("게임 로딩 끝");
    }

    private IEnumerator ClosePopupAfterDelay(float delay)
    {
        // 지정된 시간(5초) 동안 대기
        yield return new WaitForSeconds(delay);

        // 팝업을 비활성화
        Managers.Popup.CloseGameLoading();

    }
}
