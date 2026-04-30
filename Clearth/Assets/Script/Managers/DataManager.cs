using UnityEngine;

public class DataManager
{
    private MonsterDataLoader _monsterDataLoader = new MonsterDataLoader();

    public void Init()
    {
        // 몬스터 데이터 로드
        _monsterDataLoader.LoadMonsterData("json/Monsters");
    }

    // 스크립터블 오브젝트의 이름으로 몬스터 데이터를 가져오는 메서드
    public MonsterDataSO GetMonsterDataByName(string name)
    {
        return _monsterDataLoader.GetMonsterDataByName(name);
    }

    // ID로 몬스터 데이터를 가져오는 메서드
    public MonsterDataSO GetMonsterDataById(int id)
    {
        return _monsterDataLoader.GetMonsterDataById(id);
    }

}