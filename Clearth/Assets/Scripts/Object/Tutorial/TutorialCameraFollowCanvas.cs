using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class TutorialCameraFollowCanvas : MonoBehaviour
{
    public CanvasScaler canvasScaler;   // 튜토리얼이 표시되는 Canvas
    public Camera tutorialCamera;       // RenderTexture를 찍는 카메라
    public RectTransform canvasRect;    // Canvas의 RectTransform

    private Vector3 basePos;            // 기준 위치 (초기 중심)

    [SerializeField] private GameObject panelObject;

    [Header("Camera Settings")]
    [SerializeField] private float initialScale;
    [SerializeField] private float referenceOrthographicSize = 500f; //줌 수치
    [SerializeField] private float referenceYOffset = 50f; //위치 조절

    private void Awake()
    {
        if (canvasRect != null)
            initialScale = canvasRect.lossyScale.y;
    }
    void Start()
    {
        if (tutorialCamera == null) tutorialCamera = GetComponent<Camera>();

        basePos = tutorialCamera.transform.position;
    }

    void LateUpdate()
    {
        GameObject goToCheck = panelObject != null ? panelObject : canvasRect?.gameObject;

        if (goToCheck == null || tutorialCamera == null || initialScale == 0) return;

        // 2. 실제 활성화 여부에 따라 카메라 on/off
        if (!goToCheck.activeInHierarchy)
        {
            if (tutorialCamera.enabled) tutorialCamera.enabled = false;
            return;
        }
        else
        {
            if (!tutorialCamera.enabled) tutorialCamera.enabled = true;
        }

        float scaleRatio = canvasRect.lossyScale.y / initialScale;

        Vector3 canvasCenterWorld = canvasRect.TransformPoint(canvasRect.rect.center);

        float dynamicYOffset = referenceYOffset * scaleRatio;
        tutorialCamera.transform.position = new Vector3(
            canvasCenterWorld.x,
            canvasCenterWorld.y + dynamicYOffset,
            basePos.z - 1
        );

        tutorialCamera.orthographicSize = referenceOrthographicSize * scaleRatio;
    }
    public void SetTarget(Canvas canvas)
    {
        canvasScaler = canvas.GetComponent<CanvasScaler>();
        canvasRect = canvas.GetComponent<RectTransform>();
        initialScale = canvasRect.lossyScale.y; // 생성 시점의 스케일 저장
        basePos = tutorialCamera.transform.position;
    }
}
