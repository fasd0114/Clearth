using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MonsterAttackHitbox : MonoBehaviour
{
    private NewMonsterS monsterMain;

    void Awake()
    {
        // 부모 오브젝트에서 몬스터 정보를 가져옵니다.
        monsterMain = GetComponentInParent<NewMonsterS>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 대상의 HurtboxReceiver를 찾습니다.
        var receiver = other.GetComponent<HurtboxReceiver>();

        if (receiver != null && monsterMain != null)
        {
            // 몬스터 데이터의 공격력을 전달합니다.
            receiver.OnReceiveDamage(monsterMain.monsterDataSO.attack);
        }
    }
}