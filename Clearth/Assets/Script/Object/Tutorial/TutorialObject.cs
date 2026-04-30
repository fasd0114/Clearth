using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
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

        if (nowNear && !isNear)
        {
            fKeyIcon.gameObject.SetActive(true);
            isNear = true;
        }
        else if (!nowNear && isNear)
        {
            fKeyIcon.gameObject.SetActive(false);
            isNear = false;
        }

        if (isNear && Input.GetKeyDown(KeyCode.F))
        {
            TutorialUI.Instance.OpenTutorial();
        }
    }
}