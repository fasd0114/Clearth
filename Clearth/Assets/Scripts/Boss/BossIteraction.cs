using UnityEngine;
using System.Collections;

public class BossInteraction : MonoBehaviour
{
    public float interactRange = 5f;
    public SpriteRenderer fKeyIcon;
    public BossCutsceneManager cutsceneManager;

    private Transform player;
    private bool isNear = false;
    private bool hasTriggered = false;

    void Start()
    {
        if (fKeyIcon != null)
            fKeyIcon.gameObject.SetActive(false);

        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                player = found.transform;
                break;
            }
            yield return null;
        }
    }

    void Update()
    {
        if (hasTriggered || player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);
        isNear = distance < interactRange;

        if (fKeyIcon != null)
            fKeyIcon.gameObject.SetActive(isNear);

        if (isNear && Input.GetKeyDown(KeyCode.F))
        {
            hasTriggered = true;
            if (fKeyIcon != null)
                fKeyIcon.gameObject.SetActive(false);
            cutsceneManager.StartCutscene();
        }
    }
}