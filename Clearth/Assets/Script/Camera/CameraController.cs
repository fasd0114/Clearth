using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    // 플레이어 참조
    private Transform player;
    private PlayerController playerController;

    // 배경 레이어 설정
    public Transform staticFarBackground; // 고정된 원경 배경
    public Transform parallaxLayerA, parallaxLayerB; // 무한 스크롤링 중경 레이어들

    // 카메라 이동 제한 및 배경 속성
    public float minClampHeight, maxClampHeight; // 카메라 Y축 이동 제한 범위
    public float parallaxLayerWidth; // 중경 이미지의 가로 너비
    public float parallaxSpeedMultiplier; // 배경 스크롤 속도 배율

    // 위치 계산용 변수
    private Vector2 previousCameraPosition;
    private float initialParallaxY;

    // edgeGrab 보간 설정
    [SerializeField] private float edgeLerpDuration = 0.83f;
    private bool isEdgeLerping = false;
    private Vector3 lerpStartPos;
    private Vector3 lerpTargetPos;
    private float lerpTimer = 0f;

    void Start()
    {
        // 초기 위치 값 저장
        previousCameraPosition = transform.position;
        initialParallaxY = parallaxLayerA.position.y;
    }

    void Update()
    {
        // 플레이어 자동 탐색 및 할당
        if (player == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                player = playerController.transform;
            else
                return;
        }

        // 카메라 이동 로직 선택
        if (isEdgeLerping)
        {
            // 가장자리 매달리기 보간 이동 처리
            lerpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(lerpTimer / edgeLerpDuration);
            Vector3 lerpedPosition = Vector3.Lerp(lerpStartPos, lerpTargetPos, t);
            transform.position = new Vector3(lerpedPosition.x, lerpedPosition.y, transform.position.z);

            if (t >= 1f) isEdgeLerping = false;
        }
        else if (player != null)
        {
            // 일반적인 플레이어 추적 및 높이 고정
            float clampedY = Mathf.Clamp(player.position.y, minClampHeight, maxClampHeight);
            transform.position = new Vector3(player.position.x, clampedY + 2, transform.position.z);
        }

        // 원경 배경 위치 고정
        staticFarBackground.position = new Vector3(transform.position.x, staticFarBackground.position.y, staticFarBackground.position.z);

        // 중경 레이어 패럴랙스 이동 계산 및 실행
        Vector2 displacement = new Vector2(transform.position.x - previousCameraPosition.x, transform.position.y - previousCameraPosition.y);
        UpdateParallaxLayer(parallaxLayerA, displacement);
        UpdateParallaxLayer(parallaxLayerB, displacement);

        // 무한 배경 루프 체크
        ResetParallaxPosition(parallaxLayerA);
        ResetParallaxPosition(parallaxLayerB);

        // 중경 레이어 Y축 높이 유지
        parallaxLayerA.position = new Vector3(parallaxLayerA.position.x, initialParallaxY, parallaxLayerA.position.z);
        parallaxLayerB.position = new Vector3(parallaxLayerB.position.x, initialParallaxY, parallaxLayerB.position.z);

        // 현재 위치를 다음 프레임의 이전 위치로 저장
        previousCameraPosition = transform.position;
    }
    void UpdateParallaxLayer(Transform layer, Vector2 displacement)
    {
        // 이동량에 따른 배경 레이어 위치 업데이트
        layer.position += new Vector3(displacement.x * parallaxSpeedMultiplier, displacement.y, 0f);
    }

    void ResetParallaxPosition(Transform layer)
    {
        // 플레이어와의 거리에 따른 배경 레이어 재배치로 무한 루프 구현
        if (player == null) return;
        if (layer.position.x > player.position.x + parallaxLayerWidth)
            layer.position = new Vector3(layer.position.x - parallaxLayerWidth * 2, layer.position.y, layer.position.z);
        else if (layer.position.x < player.position.x - parallaxLayerWidth)
            layer.position = new Vector3(layer.position.x + parallaxLayerWidth * 2, layer.position.y, layer.position.z);
    }

    public void StartEdgeGrabLerp(Vector3 targetPosition)
    {
        // 외부에서 가장자리 매달리기 보간 이동 시작 호출
        isEdgeLerping = true;
        lerpTimer = 0f;
        lerpStartPos = transform.position;

        float clampedY = Mathf.Clamp(targetPosition.y, minClampHeight, maxClampHeight);
        lerpTargetPos = new Vector3(targetPosition.x, clampedY + 2, transform.position.z);
    }
}
