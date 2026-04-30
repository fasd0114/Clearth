using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBarController : MonoBehaviour
{
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float slideDistance = 150f;
    public float slideSpeed = 500f;

    private Vector2 topStart, bottomStart;
    private Vector2 topTarget, bottomTarget;

    void Start()
    {
        topStart = topBar.anchoredPosition + new Vector2(0, slideDistance);
        bottomStart = bottomBar.anchoredPosition - new Vector2(0, slideDistance);
        topTarget = topBar.anchoredPosition;
        bottomTarget = bottomBar.anchoredPosition;

        topBar.anchoredPosition = topStart;
        bottomBar.anchoredPosition = bottomStart;
    }

    public IEnumerator ShowBars()
    {
        while (Vector2.Distance(topBar.anchoredPosition, topTarget) > 1f)
        {
            topBar.anchoredPosition = Vector2.MoveTowards(topBar.anchoredPosition, topTarget, slideSpeed * Time.deltaTime);
            bottomBar.anchoredPosition = Vector2.MoveTowards(bottomBar.anchoredPosition, bottomTarget, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator HideBars()
    {
        while (Vector2.Distance(topBar.anchoredPosition, topStart) > 1f)
        {
            topBar.anchoredPosition = Vector2.MoveTowards(topBar.anchoredPosition, topStart, slideSpeed * Time.deltaTime);
            bottomBar.anchoredPosition = Vector2.MoveTowards(bottomBar.anchoredPosition, bottomStart, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
