using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthBar;            
    [SerializeField] private TextMeshProUGUI healthText; 

    private PlayerController playerCtrl;
    private void Start()
    {
        playerCtrl = PlayerController.Instance;
    }

    private void Update()
    {
        if (playerCtrl == null) return;

        int current = playerCtrl.CurrentHealth;
        int max = playerCtrl.MaxHealth;

        healthBar.fillAmount = Mathf.Clamp01((float)current / max);

        healthText.text = $"{current}/{max}";
    }
}