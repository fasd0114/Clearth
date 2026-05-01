using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyHitboxReceiver : MonoBehaviour
{
    AttackHitbox hitbox;

    void Awake()
    {
        hitbox = GetComponentInChildren<AttackHitbox>();
        if (hitbox == null)
            Debug.LogError("ProxyHitboxReceiver: 자식에 AttackHitbox가 없습니다.");
    }

    public void EnableHitbox()
    {
        hitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        hitbox.DisableHitbox();
    }
}
