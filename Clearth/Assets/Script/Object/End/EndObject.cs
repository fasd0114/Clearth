using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 2f;
    public SpriteRenderer fKeyIcon;

    private Transform player;
    private bool isNear = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (fKeyIcon != null)
            fKeyIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);
        bool nowNear = distance <= interactRange;

        // F키 표시 토글
        if (nowNear && !isNear)
        {
            if (fKeyIcon != null)
                fKeyIcon.gameObject.SetActive(true);
            isNear = true;
        }
        else if (!nowNear && isNear)
        {
            if (fKeyIcon != null)
                fKeyIcon.gameObject.SetActive(false);
            isNear = false;
        }

        // F키 입력 시 EndUI 열기
        if (isNear && Input.GetKeyDown(KeyCode.F))
        {
            EndUI.Instance.OpenEndUI();
        }
    }
}
