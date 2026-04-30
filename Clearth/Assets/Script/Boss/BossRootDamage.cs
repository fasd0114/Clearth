using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRootDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 2;               // 뿌리 데미지
    public string playerTag = "Player";  // 플레이어 태그
    public bool singleHit = true;        // 다중 피격 여부
    private bool hasHit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit && singleHit) return;

        if (other.CompareTag(playerTag))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            if (singleHit)
                hasHit = true;
        }
    }
}
