using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;

    bool _pressed = false;

    public void OnUpdate()
    {
        // 마우스 관련 동작만 UI 위에서 막기
        bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // 🔹 키 입력은 항상 감지하도록 분리
        if (KeyAction != null)
            KeyAction.Invoke();

        // 🔹 마우스 입력은 UI 위에서만 막기
        if (MouseAction != null && !pointerOverUI)
        {
            if (Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }
            else
            {
                if (_pressed)
                    MouseAction.Invoke(Define.MouseEvent.Click);
                _pressed = false;
            }
        }
    }


    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
    }
}
