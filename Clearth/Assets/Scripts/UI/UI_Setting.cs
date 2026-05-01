using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    enum Images
    {
        UIBGImage,
    }

    enum Sliders
    {
        BGSlider,
        SFXSlider
    }

    enum Buttons
    {
        Setting_BTN,
    }


    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));
        Bind<Button>(typeof(Buttons));

        Get<Image>((int)Images.UIBGImage);
        Get<Slider>((int)Sliders.BGSlider);
        Get<Slider>((int)Sliders.SFXSlider);

        Get<Button>((int)Buttons.Setting_BTN).gameObject.BindEvent(SetButton);
        //버튼에 함수를 적어주는 BindEvent**
    }

    void SetButton(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_Popup>("UI_Setting");
    }

}


