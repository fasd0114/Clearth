using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState { Sleep, Awake, Battle }
    public BossState currentState = BossState.Sleep;
    private Animator anim;
    private TreeBossAI bossAI;
    public BossHealthUI bossHealthUI;

    private Collider2D bossCollider;

    void Awake()
    {
        anim = GetComponent<Animator>();
        bossAI = GetComponent<TreeBossAI>();
        bossCollider = GetComponent<Collider2D>();

        if (bossAI != null)
            bossAI.enabled = false;
        if (bossCollider != null)
            bossCollider.enabled = false;
        if (bossHealthUI != null)
            bossHealthUI.HideUI();
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case BossState.Awake:
                // 상태가 Awake로 애니메이션 실행
                if (anim != null) anim.SetTrigger("WakeUp");
                break;

            case BossState.Battle:
                // 배틀 로직 수행
                if (bossAI != null) bossAI.enabled = true;
                if (bossCollider != null) bossCollider.enabled = true;
                if (bossHealthUI != null) bossHealthUI.ShowBossUI(bossAI);
                break;
        }
    }
}
