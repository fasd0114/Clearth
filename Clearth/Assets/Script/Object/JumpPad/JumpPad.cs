using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class JumpPad : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 15f; // 튕겨올라갈 힘
    public string playerTag = "Player"; // 플레이어 태그
    public string animationTrigger = "Activate"; // 애니메이션 트리거

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        Animator playerAnim = other.GetComponent<Animator>();
        if (playerAnim == null) return;

        AnimatorStateInfo state = playerAnim.GetCurrentAnimatorStateInfo(0);

        if (!(state.IsName("Jumping") || state.IsName("Falling")))
            return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Managers.Sound.Play("JumpPad", 0.5f);
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // 애니메이션 트리거
        if (anim != null)
        {
            anim.SetTrigger(animationTrigger);
        }
    }
}
