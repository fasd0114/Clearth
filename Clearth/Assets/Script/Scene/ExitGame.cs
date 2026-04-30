using UnityEngine;
using UnityEditor;

public class ExitGame : MonoBehaviour
{
    // 버튼 클릭 시 호출될 메서드
    public void QuitGame()
    {
        Debug.Log("게임이 종료되었습니다.");
      #if UNITY_EDITOR
        // 에디터에서 실행 중일 때는 플레이 모드를 종료
        UnityEditor.EditorApplication.isPlaying = false;
      #else
            // 실제 빌드 환경에서는 애플리케이션 종료
            Application.Quit();
      #endif
    }
}