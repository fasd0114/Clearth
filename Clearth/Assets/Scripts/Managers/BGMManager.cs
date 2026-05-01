using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(PlaySceneBGM());
    }

    IEnumerator PlaySceneBGM()
    {
        yield return new WaitForSeconds(0.1f);
        string scene = SceneManager.GetActiveScene().name;

        switch (scene)
        {
            case "TitleScene":
                Managers.Sound.PlayBgm("TitleBGM", 0.1f);
                break;

            case "Forest":
                Managers.Sound.PlayBgm("ForestBGM", 0.1f);
                break;

            case "ForestBossRoom":
                Managers.Sound.PlayBgm("ForestBossBGM", 0.1f);
                break;
        }
    }
}