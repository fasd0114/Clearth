using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer), typeof(BoxCollider2D))]
public class LaserController : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;
    public Transform sweepStart;
    public Transform sweepEnd;
    public LayerMask groundMask;

    [Header("Visual Settings")]
    public Gradient laserColor;
    public float laserWidth = 0.3f;
    public ParticleSystem endVFX;       // 바닥 충돌광

    [Header("Sweep Settings")]
    public float sweepDuration = 3f;

    [Header("Damage Settings")]
    public int damage = 2;
    public float hitInterval = 0.5f;

    private bool active = false;
    private float timer = 0f;
    private float lastHitTime = -999f;
    private LineRenderer line;
    private BoxCollider2D col;

    [Header("Texture Animation Settings")]
    [SerializeField] private float scrollSpeed = 2.0f;    
    [SerializeField] private float noiseAmplitude = 0.05f; 
    [SerializeField] private float noiseFrequency = 30f;

    private EdgeCollider2D edgeCol;
    private PolygonCollider2D polyCol;
    void Awake()
    {
        line = GetComponent<LineRenderer>();
        //col = GetComponent<BoxCollider2D>();
        //edgeCol = GetComponent<EdgeCollider2D>();
        polyCol = GetComponent<PolygonCollider2D>();

        line.enabled = false;
        line.positionCount = 2;
        line.colorGradient = laserColor;
        line.startWidth = laserWidth;
        line.endWidth = laserWidth;

        if (col != null) col.enabled = false;
        if (endVFX != null) endVFX.Stop();
    }

    public void Activate()
    {
        active = true;
        timer = 0f;
        line.enabled = true;
        if (col != null) col.enabled = true;
        if (endVFX != null) endVFX.Play();
    }

    public void Deactivate()
    {
        active = false;
        line.enabled = false;
        if (col != null) col.enabled = false;
        if (endVFX != null) endVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void Update()
    {
        if (!active || startPoint == null || sweepStart == null || sweepEnd == null)
            return;



        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / sweepDuration);

        // Sweep 경로
        Vector3 targetPos = Vector3.Lerp(sweepStart.position, sweepEnd.position, t);
        Vector2 direction = (targetPos - startPoint.position).normalized;


        // 바닥 충돌 감지
        RaycastHit2D hit = Physics2D.Raycast(startPoint.position, direction, 20f, groundMask);

        if (polyCol != null)
        {
            float worldScaleX = transform.lossyScale.x != 0 ? transform.lossyScale.x : 1f;
            float worldScaleY = transform.lossyScale.y != 0 ? transform.lossyScale.y : 1f;

            float localHalfWidth = laserWidth * 0.5f / worldScaleY;
            Vector2[] newPath = new Vector2[4];

            if (hit.collider != null)
            {
                // 지면까지의 실제 월드 거리
                float worldDist = Vector2.Distance(startPoint.position, hit.point);
                // 부모 스케일을 반영한 로컬 거리로 변환
                float localDist = worldDist / worldScaleX;

                // 레이저 각도(로컬)를 계산하여 수평면 오프셋 산출
                float angleRad = Mathf.Atan2(direction.y, direction.x);
                // 스케일에 구애받지 않는 수평 보정값
                float denominator = Mathf.Abs(Mathf.Tan(angleRad));
                float localHorizontalOffset = localHalfWidth / (denominator < 0.01f ? 0.01f : denominator);

                // 3. 지면 밀착형 끝점 좌표 설정 (사다리꼴 형태)
                newPath[0] = new Vector2(0, localHalfWidth);                         // 1. 좌측 상단 (시작점 위)
                newPath[1] = new Vector2(localDist - localHorizontalOffset, localHalfWidth); // 2. 우측 상단 (끝점 위)
                newPath[2] = new Vector2(localDist + localHorizontalOffset, -localHalfWidth); // 3. 우측 하단 (끝점 아래)
                newPath[3] = new Vector2(0, -localHalfWidth);
            }

            polyCol.SetPath(0, newPath);
        }

        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, targetPos);

        transform.position = startPoint.position;
        transform.right = direction;

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!active || !other.CompareTag("Player")) return;
        if (Time.time - lastHitTime < hitInterval) return;

        lastHitTime = Time.time;
        var player = other.GetComponent<PlayerController>();
        if (player != null)
            player.TakeDamage(damage);
    }
}
