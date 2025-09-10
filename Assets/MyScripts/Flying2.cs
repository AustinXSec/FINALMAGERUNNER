using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Flying2 : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [Header("Carry Settings")]
    public Shroomy shroomyToCarry;   // assign your ground enemy in Inspector
    public Transform carryPoint;     // empty GameObject as hold position
    public float detectPlayerRadius = 5f; // when to drop the enemy
    private bool isCarrying = false;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 originalScale;

    // Cached refs for carrying
    private Transform carriedEnemyTransform;
    private Rigidbody2D carriedEnemyRb;
    private Shroomy carriedEnemyScript;

    public bool isDead { get; private set; } = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        rb.gravityScale = 0f;
        col.isTrigger = false;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (shroomyToCarry != null)
            PickUpShroomy(shroomyToCarry);
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isKnockedBack) return; // skip movement if knocked back

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Always move toward the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);

        // Flip sprite
        transform.localScale = new Vector3(
            Mathf.Sign(direction.x) * Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );

        // Attack only if NOT carrying
        if (!isCarrying && distanceToPlayer <= attackRange && Time.time >= lastAttackTime)
        {
            PerformAttack();
            lastAttackTime = Time.time + attackCooldown;
        }

        // Keep Shroomy locked while carrying
        if (isCarrying && carriedEnemyTransform != null && carryPoint != null)
            carriedEnemyTransform.position = carryPoint.position;

        // Drop Shroomy if player is close enough
        if (isCarrying && distanceToPlayer <= detectPlayerRadius)
            DropEnemy();
    }

    private void PickUpShroomy(Shroomy shroomy)
    {
        carriedEnemyTransform = shroomy.transform;
        carriedEnemyScript = shroomy;
        carriedEnemyRb = shroomy.GetComponent<Rigidbody2D>();

        if (carriedEnemyRb != null)
        {
            carriedEnemyRb.isKinematic = true;
            carriedEnemyRb.velocity = Vector2.zero;
        }

        shroomy.enabled = false; // disables Shroomy's movement/attack logic
        carriedEnemyTransform.position = carryPoint.position;
        carriedEnemyTransform.parent = carryPoint;

        isCarrying = true;
    }

    private void DropEnemy()
    {
        if (carriedEnemyTransform == null) return;

        carriedEnemyTransform.parent = null;

        if (carriedEnemyRb != null)
            carriedEnemyRb.isKinematic = false;

        if (carriedEnemyScript != null)
            carriedEnemyScript.enabled = true;

        isCarrying = false;
    }

    private void PerformAttack()
    {
        animator?.SetTrigger("Attack");

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead)
                playerHealth.TakeDamage(attackDamage, transform);
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator?.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDir = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir));
        }

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = direction * knockbackForce;
            timer += Time.deltaTime;
            yield return null;
        }
        isKnockedBack = false;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator?.SetBool("isDead", true);

        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;
        col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectPlayerRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
