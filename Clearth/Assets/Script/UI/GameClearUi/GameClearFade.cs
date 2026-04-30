using UnityEngine;
using System.Collections; // 코루틴에 필요한 네임스페이스 추가

public class GameClearFade : MonoBehaviour
{
    public GameObject popup; // 팝업으로 사용할 UI 오브젝트
    private static bool _hasActivated = false; // 팝업이 이미 활성화되었는지 확인하는 변수 (게임 전체에서 유지)

    void OnEnable()
    {
        // 팝업이 한 번도 활성화된 적이 없을 때만 실행
        if (!_hasActivated)
        {
            _hasActivated = true; // 팝업 활성화 상태 기록
            StartCoroutine(ClosePopupAfterDelay(1f));
            Debug.Log("팝업이 최초로 활성화되었습니다.");
        }
        else
        {
            Debug.Log("팝업은 이미 활성화된 상태입니다. 다시 실행되지 않습니다.");
        }
    }

    private IEnumerator ClosePopupAfterDelay(float delay)
    {
        // 지정된 시간(1초) 동안 대기
        yield return new WaitForSeconds(delay);

        // 팝업을 비활성화
        Managers.Popup.CloseGameClear();
    }
}
