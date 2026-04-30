using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    //public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic("Title");
    }

    public void PlayMusic(string name)
    {
        //Sound s = Array.Find(musicSounds, x => x.name == name);
        /*
        if (musicSource.clip == s.clip && musicSource.isPlaying)
        {
            return;  // 현재 배경음악이 이미 재생 중이면 재생하지 않음
        }

        if (s == null)
        {
            Debug.Log("소리를 찾지 못 했음.");
        }

        else
        {
            //musicSource.clip = s.clip;
            musicSource.Play();
        }
        */
    }

    public void PlaySFX(string name)
    {
        //Sound s = Array.Find(sfxSounds, x => x.name == name);
        /*
        if (s == null)
        {
            Debug.Log("소리를 찾지 못 했음.");
        }

        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
        */
    }
    /*뮤트 기능
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }
    */

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    // SFX 볼륨을 반환하는 메서드
    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
