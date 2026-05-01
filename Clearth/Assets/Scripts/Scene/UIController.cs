using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public Slider _musicSlider, _sfxSlider;

    public void Start()
    {
        _musicSlider.value = AudioManager.instance.GetMusicVolume();
        _sfxSlider.value = AudioManager.instance.GetSFXVolume();
    }

    /*¹ÂÆ®±â´É
    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
    }
    */

    public void MusicVolume()
    {
        //Debug.Log("Music Volume: " + _musicSlider.value);
        AudioManager.instance.MusicVolume(_musicSlider.value);
    }

    public void SFXVolume()
    {
        //Debug.Log("SFX Volume: " + _sfxSlider.value);
        AudioManager.instance.SFXVolume(_sfxSlider.value);
    }
}
