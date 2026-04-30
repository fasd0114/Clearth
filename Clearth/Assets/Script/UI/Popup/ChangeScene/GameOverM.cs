using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverM : MonoBehaviour
{
    // 타이틀 씬 이름 또는 빌드 인덱스
    public string titleSceneName = "TitleScene"; // 타이틀 씬 이름 (필요시 수정)

    // 타이틀 씬으로 돌아가는 함수
    public void ReturnToTitle()
    {
        Managers.Popup.RealAllClosePopup();
        Time.timeScale = 1f;  // 시간 0으로 설정
        Managers.GM.gameState = GameState.restartGame;
        SceneManager.LoadScene(titleSceneName);  // 타이틀 씬으로 전환
    }
}