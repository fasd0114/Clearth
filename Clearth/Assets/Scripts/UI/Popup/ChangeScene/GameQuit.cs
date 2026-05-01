using UnityEngine;
using UnityEngine.SceneManagement;

public class GameQuit : MonoBehaviour
{
    // 버튼을 눌렀을 때 호출되는 메서드
    public void QuitGame()
    {
        // 에디터에서는 애플리케이션을 종료하지 않도록 처리
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // 게임을 종료
#endif
    }
}
