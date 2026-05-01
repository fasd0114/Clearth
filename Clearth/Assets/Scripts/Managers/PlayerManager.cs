using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{

    private SpriteRenderer spriteRenderer;

    // 플레이어 컨트롤러에서 사용할 이동 관련 변수
    public float moveSpeed = 5.0f;//이동속도
    public float dashSpeed = 4f; // 대쉬 속도
    public float dashDuration = 0.3f; // 대쉬 지속 시간
    public bool isDashing = false;// 대쉬 여부
    public float transparencyFadeTime = 0.1f; // 투명화가 진행되는 시간
    public float transparencyHoldTime = 0.5f; // 투명화가 완료된 후 유지되는 시간

    //플레이어 어택에서 사용할 변수 
    public int meleeAttackDamage = 1; //플레이어 데미지
    public float comboResetTime = 1f;//콤보 리셋 시간
    public float attackRadius = 1.6f;//공격 범위
    public float attackAngle = 140f;//공걱의 부채꼴 각도

    public int comboStep = 0;         // 현재 콤보 단계
    public bool isAttacking = false;  // 공격 중인지 여부
    public float lastAttackTime;      // 마지막 공격 시간
    public bool canChainCombo = false; // 콤보 연결 가능 여부

    //공격범위 시각화를 위한 코드(이후 삭제 가능)
    public bool isShowingAttackRange = false; 

    //Player스크립트에서 사용할 수치값
    public Vector2 inputVec;
    public int MaxHP = 100;
    public int hp = 3;
    public bool inDamage = false;
    public bool canMove = false;



    //플레이어의 이동을 멈추는 함수
    public void DisableMovement()
    {
        canMove = false;
    }

    //플레이어의 이동을 다시 시작하는 함수
    public void EnableMovement()
    {
        canMove = true;
    }


    // 투명화 상태를 관리하는 코루틴
    public IEnumerator BecomeTransparent()
    {
        Color originalColor = Color.white;

        // 투명화 시작
        float elapsedTime = 0f;
        while (elapsedTime < transparencyFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0.5f, elapsedTime / transparencyFadeTime); // 투명도를 점진적으로 변경
            Color transparentColor = originalColor;
            transparentColor.a = alpha;
            spriteRenderer.color = transparentColor;
            yield return null; // 다음 프레임까지 대기
        }

        // 투명 상태 유지
        yield return new WaitForSeconds(transparencyHoldTime);

        // 원래 상태로 복구
        elapsedTime = 0f;
        while (elapsedTime < transparencyFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 1f, elapsedTime / transparencyFadeTime); // 투명도를 원래대로 복구
            Color transparentColor = originalColor;
            transparentColor.a = alpha;
            spriteRenderer.color = transparentColor;
            yield return null; // 다음 프레임까지 대기
        }
    }


    // 플레이어 수치 초기 설정
    public void Init()
    {
        canMove = true;
    }
}