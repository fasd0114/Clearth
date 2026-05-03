using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private int damage = 2; // 트랩 데미지 설정

    private void OnTriggerStay2D(Collider2D other)
    {
        var receiver = other.GetComponent<HurtboxReceiver>();

        if (receiver != null)
        {
            // 플레이어 무적 상태면 PlayerController.TakeDamage에서 알아서 씹힘
            receiver.OnReceiveDamage(damage);
        }
    }
}