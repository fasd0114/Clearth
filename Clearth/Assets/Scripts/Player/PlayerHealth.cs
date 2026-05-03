using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    [Header("Invincibility")]
    [SerializeField] private float invincibleDuration = 3f;
    [SerializeField] private float flashInterval = 0.1f;

    private bool isInvincible;
    private bool isDead;
    private SpriteRenderer spriteRenderer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        if (isInvincible || isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        Debug.Log($"[Health] HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Managers.Popup.OpenGameOver();
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibleDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}