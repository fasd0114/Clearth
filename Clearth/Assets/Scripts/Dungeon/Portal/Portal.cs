using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private BoxCollider2D portalTrigger;
    // 타이틀 씬 이름 또는 빌드 인덱스
    public string ForestBossRoom = "ForestBossRoom"; // 타이틀 씬 이름 (필요시 수정)

    public Vector2 spawnPosition;

    private void Awake()
    {
        portalTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
            Managers.Popup.RealAllClosePopup();
            Managers.Popup.OpenGameLoading();
            SceneManager.LoadScene(ForestBossRoom);  // 타이틀 씬으로 전환


        }
    }

}


