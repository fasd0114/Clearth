using UnityEngine;

public class BossFruit : MonoBehaviour
{
    [Header("Seed Settings")]
    public int damage = 2;                  
    public float destroyDelay = 0.5f;       
    public LayerMask groundLayer;           
    public Animator anim;

    private bool hasLanded = false;
    private bool hasHitPlayer = false;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded || hasHitPlayer) return;

        if (other.CompareTag("Player"))
        {
            hasHitPlayer = true;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Managers.Sound.Play("SeedHit", 0.7f);

            Destroy(gameObject); // 애니메이션 없이 바로 삭제
            return;
        }

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            hasLanded = true;

            if (anim != null)
                anim.SetTrigger("Break");

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            if (col != null)
                col.enabled = false;

            Managers.Sound.Play("SeedHit", 0.3f);

            Destroy(gameObject, destroyDelay);
        }
    }
}
