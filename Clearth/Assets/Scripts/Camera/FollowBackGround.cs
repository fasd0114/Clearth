using UnityEngine;

public class FollowBackground : MonoBehaviour
{
    public Transform player;         // 플레이어의 위치
    public Vector3 offset;           // 배경과 플레이어 사이의 오프셋

    void Start()
    {
        // 배경이 카메라와 일정한 위치 차이를 유지하도록 설정
        offset = transform.position - player.position;
    }

    void Update()
    {
        // 배경의 위치는 고정된 오프셋을 유지하며, 카메라가 플레이어를 추적하더라도 배경은 이동하지 않음
        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
    }
}
