using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverT : MonoBehaviour
{
    // 타이틀 씬 이름 또는 빌드 인덱스
    public string SceneName = "TitleScene"; // 게임 시작 

    // 타이틀 씬으로 돌아가는 함수
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;  // 시간 다시 정상으로 설정
        Managers.GM.gameState = GameState.restartGame;
        SceneManager.LoadScene(SceneName);  // 타이틀 씬으로 전환
    }
}