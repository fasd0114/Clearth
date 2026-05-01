using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    public Image fadeImage; // Canvas 최상단에 깔릴 흰색 이미지

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        fadeImage.color = new Color(1, 1, 1, 0);
    }

    public IEnumerator FadeIn(float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, time / duration);
            fadeImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            fadeImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
}
