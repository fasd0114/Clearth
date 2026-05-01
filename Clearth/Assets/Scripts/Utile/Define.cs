using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    public enum Scene
    {
        Unknown,
        Game,
        MainMenu,
        GameScene
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum UIEvent
    {
        Click,
        Drag
    }

    public enum MouseEvent
    {
        Press,
        Click
    }

    public enum CameraMode
    {
        //게임 설정에 맞는 뷰를 입력
    }
}
