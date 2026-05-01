using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager
{
    [System.Serializable]
    public class Pool
    {
        public string tag;        // 풀의 태그
        public GameObject prefab; // 풀에서 관리할 프리팹
        public int size;          // 미리 생성할 객체의 수
    }

    public List<Pool> pools;  // 여러 풀을 관리하기 위한 리스트
    public Dictionary<string, Queue<GameObject>> poolDictionary;  // 태그로 접근하는 풀 딕셔너리

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // 각 풀에 대해 객체를 미리 생성하고 큐에 넣는다.
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // 비활성화 상태로
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool); // 딕셔너리에 추가
        }
    }

    private GameObject Instantiate(GameObject prefab)
    {
        throw new NotImplementedException();
    }

    // 객체를 풀에서 가져오는 함수
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("풀에 없는 태그: " + tag);
            return null;
        }

        // 풀에 남아있는 객체가 있는지 확인
        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("풀에 남아 있는 객체가 없습니다: " + tag);
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // 풀에서 가져온 객체를 활성화하고 위치와 회전 설정
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 객체가 풀로 돌아가기 전, 필요한 초기화 작업 수행
        // (예: 체력 초기화, 위치 재설정 등 필요한 초기화)

        return objectToSpawn;
    }

    // 객체를 다시 풀로 반환하는 함수
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("풀에 없는 태그: " + tag);
            return;
        }

        // 객체를 비활성화하고 다시 큐에 추가
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
