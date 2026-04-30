using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VineHangHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerController playerController;
    private VineController currentVine;
    private Animator anim;
    private bool isHanging = false;
    private Vector3 hangOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>(); // Animator 연결
    }

    void Update()
    {
        if (isHanging && currentVine != null)
        {
            // 회전 각도 반영 위치 계산
            Vector3 rotatedOffset = currentVine.transform.rotation * hangOffset;
            transform.position = currentVine.transform.position + rotatedOffset;

            // 각도 동기화
            transform.rotation = currentVine.transform.rotation;

            if (Input.GetButtonDown("Jump"))
            {
                DetachFromVine();
            }
        }
    }

    public void AttachToVine(VineController vine)
    {
        if (isHanging) return;
        currentVine = vine;
        isHanging = true;
        playerController.isHangingFromVine = true;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        // 플레이어가 덩쿨의 어느 쪽에 있는지 계산
        float side = transform.position.x - vine.transform.position.x;

        hangOffset = new Vector3(-1f, -5f, 0);

        // 오른쪽에서 매달릴 경우 반전
        if (side > 0)
            hangOffset.x *= -1;

        int swingDir = (side > 0) ? -1 : 1;
        currentVine.StartSwing(swingDir);

        if (anim != null)
            anim.SetBool("isVineHanging", true);
    }
    public void DetachFromVine(Vector2 vineVelocity = default)
    {
        if (!isHanging) return;
        isHanging = false;
        playerController.isHangingFromVine = false;
        playerController.LockMovement();

        rb.isKinematic = false;
        rb.gravityScale = 3f;

        if (currentVine != null)
        {
            float angle = currentVine.transform.eulerAngles.z * Mathf.Deg2Rad;

            // 접선 방향 (덩쿨의 회전에 수직한 방향)
            Vector2 tangentDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // 덩쿨의 진자 움직임에 따른 현재 스윙 부호
            float swingDir = Mathf.Sign(Mathf.Cos((Time.time - currentVine.startTime) * currentVine.swingSpeed));

            // 덩쿨의 시작 방향에 따라 전체 부호 조정
            swingDir *= currentVine.GetStartDirection();

            // 최종 점프 벡터 계산
            Vector2 jumpVelocity = tangentDir * swingDir * 8f + Vector2.up * 8f;
            rb.velocity = jumpVelocity;

            currentVine.StopSwing();
        }

        currentVine = null;
        transform.rotation = Quaternion.identity;

        if (anim != null)
            anim.SetBool("isVineHanging", false);
    }
   }
