using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    [Tooltip("입력할 대미지 값")]
    public int damage = 1;

    Collider2D col;
    HashSet<NewMonsterS> hitMonsters = new HashSet<NewMonsterS>();
    HashSet<TreeBossAI>hitBoss = new HashSet<TreeBossAI>();

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    public void EnableHitbox()
    {
        hitMonsters.Clear();
        hitBoss.Clear();
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!col.enabled) return;

        // 몬스터 처리
        if (other.CompareTag("Monsters"))
        {
            bool hitSomething = false;

            NewMonsterS monster = other.GetComponent<NewMonsterS>();
            if (monster != null && !hitMonsters.Contains(monster))
            {
                hitMonsters.Add(monster);
                Vector3 hitDir = other.transform.position - transform.root.position;
                monster.TakeDamage(damage, hitDir);
                hitSomething = true;
            }

            TreeBossAI Boss = other.GetComponent<TreeBossAI>();
            if (Boss != null && !hitBoss.Contains(Boss))
            {
                hitBoss.Add(Boss);
                Vector3 hitDir = other.transform.position - transform.root.position;
                Boss.TakeDamage(damage, hitDir);
                hitSomething = true;
            }

            if (hitSomething)
                Managers.Sound.Play("MonsterHit", 0.5f);
        }

        // **상호작용 오브젝트 처리**
        if (other.CompareTag("Interactive"))
        {
            BreakableObject obj = other.GetComponent<BreakableObject>();
            if (obj != null)
            {
                // 플레이어 방향 가져오기
                float dir = Mathf.Sign(PlayerController.Instance.transform.localScale.x);
                obj.Fall(new Vector2(dir, 0));
            }
        }
    }

}
