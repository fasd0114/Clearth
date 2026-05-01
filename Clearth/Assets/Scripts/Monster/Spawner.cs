using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public string targetTag = "Player";  // 플레이어 태그
    public float spawnDistance = 30f;     // 몬스터 소환 거리
    public MonsterDataSO[] monsterDataArray;  // 스크립터블 오브젝트 배열로 여러 몬스터 데이터 받아오기
    public int spawnCount = 3;  // 소환할 몬스터의 수

    private List<GameObject> spawnedMonsters = new List<GameObject>();  // 소환된 몬스터들을 추적하기 위한 리스트

    void Update()
    {
        // "Player" 태그를 가진 오브젝트가 일정 범위 내에 들어오면 몬스터 소환
        GameObject player = GameObject.FindWithTag(targetTag);

        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= spawnDistance)
        {
            // 소환된 몬스터가 없다면 새로 소환
            if (spawnedMonsters.Count == 0)
            {
                SpawnMonsters();
            }
        }
    }

    void SpawnMonsters()
    {
        if (monsterDataArray.Length > 0)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, monsterDataArray.Length);
                MonsterDataSO selectedMonsterData = monsterDataArray[randomIndex];

                if (selectedMonsterData != null && selectedMonsterData.monsterPrefab != null)
                {
                    GameObject newMonster = Instantiate(selectedMonsterData.monsterPrefab, transform.position, Quaternion.identity);
                    spawnedMonsters.Add(newMonster);
                    Debug.Log($"{selectedMonsterData.monsterName}이(가) 소환되었습니다!");

                    // 보스라면 체력 UI 표시
                    if (newMonster.GetComponent<BossMonster1>() != null)
                    {
                        StartCoroutine(SetupBossUI(newMonster));
                    }
                }
                else
                {
                    Debug.LogWarning("선택된 몬스터의 프리팹이 설정되지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("몬스터 데이터 배열이 비어 있습니다.");
        }
    }
    private IEnumerator SetupBossUI(GameObject bossObj)
    {
        yield return null; // 한 프레임 대기 (BossMonster1.Start() 실행됨)

        BossMonster1 boss = bossObj.GetComponent<BossMonster1>();
        if (boss != null)
        {
            BossHealthUI bossUI = FindObjectOfType<BossHealthUI>(true);
            if (bossUI != null)
            {
                //bossUI.ShowBossUI(boss);
                Debug.Log($"보스 UI 연결 완료: {boss.monsterDataSO.monsterName}, 체력 {boss.health}");
            }
        }
    }
}
