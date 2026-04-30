using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterDataLoader
{
    // ILoader 인터페이스 - 데이터를 로딩하는 공통된 방식 정의
    public interface ILoader<Key, Value>
    {
        // 데이터를 Dictionary 형태로 반환하기 위한 메서드
        Dictionary<Key, Value> MakeDictionary();
    }

    // MonsterData 클래스 - 개별 몬스터의 데이터를 담는 클래스
    [Serializable]  // JSON 파일로부터 데이터를 직렬화 및 역직렬화하기 위한 속성
    public class MonsterData
    {
        public int id;              // 몬스터의 고유 ID
        public string name;         // 몬스터의 이름
        public int health;          // 몬스터의 체력
        public int attack;          // 몬스터의 공격력
        public int attackRange;     // 몬스터의 공격 범위
        public float range;         // 몬스터의 감지 범위
        public float speed;         // 몬스터의 이동 속도
    }

    // MonsterDataList 클래스 - JSON 파일로부터 데이터를 파싱하여 MonsterData 객체들을 관리하는 클래스
    [Serializable]
    public class MonsterDataList : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters;  // JSON 파일에서 불러온 몬스터 데이터 목록

        // MonsterData 객체들을 Dictionary로 변환하여 ID를 키로 사용, 빠르게 접근할 수 있도록 설정
        public Dictionary<int, MonsterData> MakeDictionary()
        {
            Dictionary<int, MonsterData> dic = new Dictionary<int, MonsterData>();
            foreach (MonsterData data in monsters)
            {
                dic.Add(data.id, data);  // 몬스터 ID를 키로, 몬스터 데이터를 값으로 추가
            }
            return dic;
        }
    }

    // MonsterDataSO 객체를 담을 딕셔너리 - ScriptableObject로 변환한 몬스터 데이터를 저장
    private Dictionary<int, MonsterDataSO> monsterDataSOList = new Dictionary<int, MonsterDataSO>();

    // JSON 파일에서 몬스터 데이터를 로드하는 메서드
    public void LoadMonsterData(string path)
    {
        // Resource 매니저를 통해 지정된 경로에서 JSON 데이터를 TextAsset으로 불러옴
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        if (textAsset == null)
        {
            // 파일이 없을 경우 에러 메시지 출력
            Debug.LogError($"'{path}' 경로에서 파일을 찾을 수 없습니다.");
            return;
        }

        // JSON 파일의 텍스트 데이터를 MonsterDataList로 역직렬화 (JSON -> 객체 변환)
        MonsterDataList loader = JsonUtility.FromJson<MonsterDataList>(textAsset.text);

        // 역직렬화된 몬스터 데이터를 Dictionary로 변환
        Dictionary<int, MonsterData> monsterDataDic = loader.MakeDictionary();

        // 변환된 데이터를 ScriptableObject로 다시 변환하여 저장
        PopulateMonsterSO(monsterDataDic);
    }

    // Dictionary로 관리되는 MonsterData를 ScriptableObject로 변환하는 메서드
    private void PopulateMonsterSO(Dictionary<int, MonsterData> monsterDataDic)
    {
        // Dictionary에 저장된 몬스터 데이터를 순회하며 ScriptableObject로 변환
        foreach (KeyValuePair<int, MonsterData> entry in monsterDataDic)
        {
            // 새로운 MonsterDataSO 객체 생성
            MonsterDataSO monsterDataSO = ScriptableObject.CreateInstance<MonsterDataSO>();

            // MonsterData 객체의 데이터를 MonsterDataSO 객체에 복사
            monsterDataSO.id = entry.Value.id;
            monsterDataSO.monsterName = entry.Value.name;
            monsterDataSO.health = entry.Value.health;
            monsterDataSO.attack = entry.Value.attack;
            monsterDataSO.attackRange = entry.Value.attackRange;
            monsterDataSO.range = entry.Value.range;
            monsterDataSO.speed = entry.Value.speed;

            // 변환된 MonsterDataSO 객체를 monsterDataSOList 딕셔너리에 저장 (ID를 키로 사용)
            monsterDataSOList.Add(entry.Key, monsterDataSO);
        }
    }

    // 몬스터 이름을 통해 몬스터 데이터를 검색하는 메서드
    public MonsterDataSO GetMonsterDataByName(string name)
    {
        // monsterDataSOList에 저장된 모든 데이터를 순회하며 이름이 일치하는 몬스터를 찾음
        foreach (var monsterDataSO in monsterDataSOList.Values)
        {
            if (monsterDataSO.monsterName == name)
            {
                return monsterDataSO;  // 이름이 일치하는 경우 해당 MonsterDataSO 객체 반환
            }
        }

        // 이름을 찾을 수 없는 경우 에러 메시지 출력
        Debug.LogError($"'{name}'에 해당하는 몬스터 데이터를 찾을 수 없습니다.");
        return null;
    }

    // 몬스터 ID를 통해 몬스터 데이터를 검색하는 메서드
    public MonsterDataSO GetMonsterDataById(int id)
    {
        // 딕셔너리에서 ID로 데이터를 바로 검색, 존재할 경우 해당 데이터를 반환
        if (monsterDataSOList.TryGetValue(id, out MonsterDataSO monsterDataSO))
        {
            return monsterDataSO;
        }

        // ID를 찾을 수 없는 경우 에러 메시지 출력
        Debug.LogError($"'{id}'에 해당하는 몬스터 데이터를 찾을 수 없습니다.");
        return null;
    }
}
