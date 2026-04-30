using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    void Start()
    {
        // 오브젝트가 5초 후에 사라지게 설정
        Destroy(gameObject, 5f);
    }
}
