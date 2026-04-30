using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private void Awake()
    {
        // 씬이 로드될 때마다 호출되는 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 타이틀 씬이 로드되면 플레이어 삭제
        if (scene.name == "TitleScene")
        {
            Destroy(gameObject);  // 타이틀 씬으로 넘어갈 때 플레이어 오브젝트 삭제
        }
        else
        {
            // 타이틀 씬이 아닌 다른 씬일 경우, DontDestroyOnLoad 적용
            DontDestroyOnLoad(gameObject);  // 타이틀 씬이 아닌 경우 플레이어를 씬 전환 간 유지
        }
    }

    private void OnDestroy()
    {
        // 씬이 로드될 때마다 호출되는 이벤트 구독
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
