using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    public Animator animator;

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;
    public Transform player;

    [Header("Potion Drop")]
    public GameObject potionPrefab;         // Assign your potion prefab here
    public int minDrop = 1;                 // Minimum potions to drop
    public int maxDrop = 3;                 // Maximum potions to drop
    public float horizontalSpread = 1f;     // Horizontal random offset
    public float dropHeight = 1f;           // Height offset for dropping
    [Range(0f, 1f)]
    public float dropChance = 0.5f;         // Chance to drop potion
    public LayerMask groundLayer;           // Tilemap ground layer

    private Rigidbody2D rb;
    private Collider2D col;

    public bool isDead { get; private set; } = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 1f;
        col.isTrigger = false;
    }

    void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            Vector2 direction = player.position - transform.position;
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime)
            {
                Attack();
                lastAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Default"));
        foreach (Collider2D playerCollider in hitPlayers)
        {
            IDamageable damageable = playerCollider.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(attackDamage, transform);
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        direction.y = 0f;
        direction.Normalize();

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = new Vector2(direction.x * knockbackForce, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

   void Die()
{
    isDead = true;
    animator.SetBool("IsDead", true);

    // Stop movement
    rb.velocity = Vector2.zero;

    // Disable all colliders so he falls through the map
    foreach (Collider2D c in GetComponents<Collider2D>())
        c.enabled = false;

    // Keep gravity so he falls down naturally
    rb.gravityScale = 1f;

    // Give mana to player
    PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
    if (playerHealth != null)
        playerHealth.AddMana(20); // Adjust if needed

    // Drop potions
    if (potionPrefab != null && Random.value <= dropChance)
        DropPotions();

    // Destroy after a few seconds so it doesn’t hang around forever
    Destroy(gameObject, 1f);
}


    void DropPotions()
    {
        int dropCount = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-horizontalSpread, horizontalSpread), dropHeight, 0);
            GameObject potion = Instantiate(potionPrefab, spawnPos, Quaternion.identity);

            // Ensure Rigidbody2D exists
            Rigidbody2D rbPotion = potion.GetComponent<Rigidbody2D>();
            if (rbPotion == null)
                rbPotion = potion.AddComponent<Rigidbody2D>();

            rbPotion.bodyType = RigidbodyType2D.Dynamic;
            rbPotion.gravityScale = 1f;

            // Ensure Collider2D exists
            Collider2D colPotion = potion.GetComponent<Collider2D>();
            if (colPotion == null)
            {
                colPotion = potion.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)colPotion).radius = 0.2f;
            }

            // Tiny horizontal force for spread
            rbPotion.AddForce(new Vector2(Random.Range(-1f, 1f), 0), ForceMode2D.Impulse);

            // Add ground snap
            PotionGroundSnap snap = potion.AddComponent<PotionGroundSnap>();
            snap.groundLayer = groundLayer;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
