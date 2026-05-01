using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 클래스들을 사용하기 위해 추가

public class SubMenu : MonoBehaviour
{
    public GameObject SubMenuSet;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSubMenu();
        }
    }

    public void ToggleSubMenu()
    {
        if (SubMenuSet.activeSelf)
        {
            SubMenuSet.SetActive(false);    
            Cursor.visible = false; // 마우스 커서를 숨김
            Cursor.lockState = CursorLockMode.Locked; // 마우스를 잠금
            Time.timeScale = 1; // 게임을 계속 진행
        }
        else
        {
            SubMenuSet.SetActive(true);
            Cursor.lockState = CursorLockMode.None; // 마우스를 잠금 해제
            Cursor.visible = true; // 마우스 커서를 표시
            Time.timeScale = 0; // 게임을 일시 중지
        }
    }

    public void CloseSubMenu()
    {
        SubMenuSet.SetActive(false);
        Time.timeScale = 1; // 게임을 계속 진행
        Cursor.visible = false; // 마우스 커서를 숨김
        Cursor.lockState = CursorLockMode.Locked; // 마우스를 잠금
    }
}
