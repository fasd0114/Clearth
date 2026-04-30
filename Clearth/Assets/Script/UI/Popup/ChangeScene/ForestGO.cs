using UnityEngine;
using UnityEngine.SceneManagement;

public class ForestGO : MonoBehaviour
{
    // 타이틀 씬 이름 또는 빌드 인덱스
    public string ForestScene = "Forest"; // 타이틀 씬 이름 (필요시 수정)

    // 타이틀 씬으로 돌아가는 함수
    public void GoForest()
    {
        Managers.Popup.RealAllClosePopup();
        Time.timeScale = 1f;  // 시간 다시 정상으로 설정
        Managers.GM.gameState = GameState.gameStarted;
        SceneManager.LoadScene(ForestScene);  // 타이틀 씬으로 전환
    }
}