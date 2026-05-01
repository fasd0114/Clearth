using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineController : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAmplitude = 45f;  // 회전 각도
    public float swingSpeed = 1f;       // 속도

    public float vineLength = 5f;
    private bool isSwinging = false;    
    public float startTime;

    private int startDirection = 1;

    void Update()
    {
        if (!isSwinging)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
            return;
        }

        // 시작 방향에 따라 회전 부호 반전
        float angle = Mathf.Sin((Time.time - startTime) * swingSpeed) * swingAmplitude * startDirection;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartSwing(int direction = 1)
    {
        if (isSwinging) return;
        isSwinging = true;
        startDirection = direction;
        startTime = Time.time;
    }

    public void StopSwing()
    {
        isSwinging = false;
    }

    // 속도 계산
    public Vector2 GetCurrentVelocity()
    {
        if (!isSwinging) return Vector2.zero;

        // 각속도 (라디안/초)
        float angularVelocity = Mathf.Cos((Time.time - startTime) * swingSpeed)
                                * swingSpeed * swingAmplitude * Mathf.Deg2Rad * startDirection; ;

        // 진자 하단의 속도는 각속도 × 반지름(덩쿨 길이)
        // 방향은 덩쿨의 회전에 수직이므로 Z축 회전값으로부터 x/y 계산
        float angle = Mathf.Sin((Time.time - startTime) * swingSpeed) * swingAmplitude * Mathf.Deg2Rad * startDirection; ;
        Vector2 tangentialVelocity = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * angularVelocity * vineLength;

        return tangentialVelocity;
    }
    public int GetStartDirection()
    {
        return startDirection;
    }
}
