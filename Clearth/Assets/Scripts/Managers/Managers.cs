using UnityEngine;
using UnityEngine.SceneManagement;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();
    GameManager _game = new GameManager();
    PlayerManager _player = new PlayerManager();
    UIPopupManager _popup = new UIPopupManager();
    DataManager _data = new DataManager();  



    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static GameManager GM { get { Init(); return Instance._game; } }
    public static PlayerManager Player { get { Init(); return Instance._player; } }
    public static UIPopupManager Popup { get { Init(); return Instance._popup; } }
    public static DataManager Data { get { Init(); return Instance._data; } }

    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();  // 매 프레임 입력 업데이트
    }

    void FixedUpdate()
    {
        _game.HandleGameState();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._pool.Init();
            s_instance._sound.Init();
            s_instance._player.Init();
            s_instance._data.Init();

            s_instance._ui.InitializeButtonSounds();
        }
    }


    public static void Clear()
    {
        Input.Clear();
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
    }
}
