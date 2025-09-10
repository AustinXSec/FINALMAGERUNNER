using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Health UI")]
    public Image healthBar;
    public Text healthText;

    [Header("Mana UI")]
    public Image manaBar;
    public Text manaText;
    public float maxMana = 300f;
    private float currentMana = 0f;

    [Header("Mana Sounds")]
    public AudioClip manaFullSound;
    [Range(0f, 1f)] public float manaFullVolume = 0.7f;

    public AudioClip manaEmptySound;
    [Range(0f, 1f)] public float manaEmptyVolume = 0.7f;

    public AudioSource audioSource;

    private bool isDead = false;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        UpdateHealthUI();
        UpdateManaUI();
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        animator?.SetTrigger("Hurt");

        // Play hurt sound
        GetComponent<HeroKnight>()?.PlayHurtSound();

        // Apply knockback
        if (attacker != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = new Vector2(direction.x * knockbackForce, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Play death animation & sound
        animator?.SetTrigger("Death");
        GetComponent<HeroKnight>()?.PlayDeathSound();

        // Freeze movement
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Disable player scripts
        var hero = GetComponent<HeroKnight>();
        if (hero != null) hero.enabled = false;

        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        // Start scene reload after delay (so animation/sound plays)
        StartCoroutine(ReloadSceneAfterDelay(7f));
    }

    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1f; // ensure time scale is normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";
    }

    public void AddMana(float amount)
    {
        if (PlayerCombat.Instance != null && PlayerCombat.Instance.specialActive)
            return;

        currentMana += amount;
        if (currentMana > maxMana)
            currentMana = maxMana;

        UpdateManaUI();

        if (currentMana >= maxMana)
        {
            if (audioSource != null && manaFullSound != null)
                audioSource.PlayOneShot(manaFullSound, manaFullVolume);

            PlayerCombat.Instance?.EnableSpecialAttack();
        }
    }

    public void ResetMana()
    {
        currentMana = 0f;
        UpdateManaUI();

        if (audioSource != null && manaEmptySound != null)
            audioSource.PlayOneShot(manaEmptySound, manaEmptyVolume);
    }

    private void UpdateManaUI()
    {
        if (manaBar != null)
            manaBar.fillAmount = currentMana / maxMana;

        if (manaText != null)
            manaText.text = $"{currentMana:0}/{maxMana:0}";
    }
}
