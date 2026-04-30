using UnityEngine;
using System.Collections;

public class BreakableObject : MonoBehaviour
{
    private bool isFallen = false;
    public float fallDuration = 0.5f; // 쓰러지는 시간
    public GameObject colliderChild;  // 🔹 자식 콜라이더 오브젝트 (에디터에서 지정)

    // 피봇 기준 회전
    public void Fall(Vector2 playerDir)
    {
        if (isFallen) return;
        isFallen = true;

        Managers.Sound.Play("InteractiveTree", 0.2f);

        float targetAngle = playerDir.x > 0 ? -90f : 90f;
        StartCoroutine(RotateOverTime(targetAngle, fallDuration));
    }

    private IEnumerator RotateOverTime(float targetAngle, float duration)
    {
        Transform target = transform;

        Quaternion startRot = target.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        target.rotation = endRot; // 회전 마무리 보정

        // 🔹 회전이 끝나면 자식 오브젝트(콜라이더) 활성화
        if (colliderChild != null)
            colliderChild.SetActive(true);
    }

    private void Start()
    {
        // 🔹 시작 시 자식 오브젝트를 비활성화
        if (colliderChild != null)
            colliderChild.SetActive(false);
    }
}
