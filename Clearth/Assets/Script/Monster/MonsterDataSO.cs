using UnityEngine;

//몬스터의 스크립터블 오브젝트 ai작성과 사용시에는 
//몬스터의 이름을 프리팹의 이름과 일치시켜서 사용할것
//AI 작성시에는 몬스터 AI의 Start함수를 참조할것
[CreateAssetMenu(fileName = "MonsterDataSO", menuName = "ScriptableObjects/MonsterDataSO", order = 1)]
public class MonsterDataSO : ScriptableObject
{
    public string monsterName;  // 몬스터 이름 (프리팹 이름과 비교)
    public int id;              // 몬스터 ID
    public int health;          // 체력
    public int attack;          // 공격력
    public int attackRange;
    public float range;         // 공격 범위
    public float speed;         // 이동 속도

    public GameObject monsterPrefab;  // 몬스터 프리팹을 저장할 필드
}