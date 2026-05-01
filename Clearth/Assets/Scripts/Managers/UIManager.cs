using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager
{
    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;

    private const string buttonSoundName = "Button";
    public UIManager()
    {
        // 씬이 로드될 때마다 자동으로 버튼 사운드 등록
        SceneManager.sceneLoaded += (scene, mode) => InitializeButtonSounds();
    }
    public void InitializeButtonSounds()
    {
        Button[] allButtons = GameObject.FindObjectsOfType<Button>(true);
        int count = 0;

        foreach (Button btn in allButtons)
        {
            // 이미 동일한 Play 함수가 등록되어 있는지 중복 검사
            bool alreadyHasSound = false;
            int eventCount = btn.onClick.GetPersistentEventCount();

            for (int i = 0; i < eventCount; i++)
            {
                var target = btn.onClick.GetPersistentTarget(i);
                var methodName = btn.onClick.GetPersistentMethodName(i);
                if (methodName == "Play" && target != null && target.ToString().Contains("Sound"))
                {
                    alreadyHasSound = true;
                    break;
                }
            }

            if (!alreadyHasSound)
            {
                btn.onClick.AddListener(() => Managers.Sound.Play(buttonSoundName, 1f));
                count++;
            }
        }

        Debug.Log($"[UIManager] {count}개의 버튼에 '{buttonSoundName}' 사운드 등록 완료");
    }

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");
        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        // 호출 횟수 확인을 위한 디버그 로그 추가
        Debug.Log($"ShowPopupUI 호출됨: {name}");

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

        return popup;
    }


    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
    }

}
