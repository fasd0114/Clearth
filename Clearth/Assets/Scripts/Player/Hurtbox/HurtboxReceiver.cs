using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtboxReceiver : MonoBehaviour
{
    PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null)
            Debug.LogError("HurtboxReceiver: PlayerController가 부모에 없습니다!");

        // Collider를 Trigger 모드로 강제 설정
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
        => player?.TryDamageFrom(other);

    void OnTriggerStay2D(Collider2D other)
        => player?.TryDamageFrom(other);
}