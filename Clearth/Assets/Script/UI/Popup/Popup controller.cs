using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popupcontroller : MonoBehaviour
{

    // Update is called once per frame
    void OnEnable()
    {
        Managers.Input.KeyAction -= OpenPopup;
        Managers.Input.KeyAction += OpenPopup;
    }
    void OnDisable()
    {
        Managers.Input.KeyAction -= OpenPopup;
    }

    void OpenPopup()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Managers.Popup.OpenGamePause();
            Debug.Log("¿ÀÇÂÆË¾÷");
        }
    }
}
