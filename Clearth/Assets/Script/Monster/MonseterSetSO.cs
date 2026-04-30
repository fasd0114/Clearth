using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSetSO", menuName = "SciptableObject/MonsterSetSO")]
public class MonsterSetSO : ScriptableObject
{
    public MonsterDataSO[] monsters;

    public MonsterDataSO GetMonster(int index)
    {
        if (index >= 0 && index < monsters.Length)
        {
            return monsters[index];
        }
        else
        {
            Debug.LogWarning("Invalid index for monster array");
            return null;
        }
    }
}
