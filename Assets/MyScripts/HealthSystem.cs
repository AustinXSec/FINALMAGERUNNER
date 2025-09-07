using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance;

    [Header("Health UI")]
    public Image healthBar;      // Assign your health bar Image here
    public Text healthText;      // Optional, assign if you want text display

    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Take damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();
        if (currentHealth == 0)
        {
            Die();
        }
    }

    // Heal damage
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();
    }

    // Update health bar and text
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float ratio = currentHealth / maxHealth;
            healthBar.fillAmount = ratio;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:0}/{maxHealth:0}";
        }
    }

    // Called when health reaches 0
    private void Die()
    {
        Debug.Log("Player is dead!");
        // Implement death logic here (e.g., respawn, game over)
    }
}
