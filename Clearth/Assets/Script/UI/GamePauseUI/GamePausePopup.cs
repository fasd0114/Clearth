using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePausePopup : UI_Popup
{
    public override void Init()
    {
        base.Init();
    }

    public void Continue()
    {
        Managers.Popup.CloseGamePause();
    }
}
