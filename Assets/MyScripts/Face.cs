using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Face : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 80;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float hoverHeight = 2f;         // minimum height from the ground
    public LayerMask groundLayer;
    public float randomMoveRadius = 3f;    // radius for idle wandering
    public float randomMoveInterval = 2f;
    public float detectionRadius = 5f;     // player detection radius

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 15;
    public float attackCooldown = 1f;

    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.2f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Mana Reward")]
    public int manaReward = 15;

    private Rigidbody2D rb;
    private Vector3 originalScale;
    private Vector2 movement;
    private bool isKnockedBack = false;
    public bool isDead { get; private set; } = false;
    private bool canAttack = true;
    private Vector2 randomTarget;
    private float randomMoveTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        rb.gravityScale = 0f;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        PickRandomTarget();
    }

    void Update()
    {
        if (isDead || player == null) return;

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Decide movement
        if (!isKnockedBack)
        {
            if (distanceToPlayer <= detectionRadius) // only follow if inside detection radius
            {
                movement = directionToPlayer.normalized;
            }
            else
            {
                // Random wandering
                randomMoveTimer += Time.deltaTime;
                if (randomMoveTimer >= randomMoveInterval)
                {
                    PickRandomTarget();
                    randomMoveTimer = 0f;
                }

                movement = ((Vector2)randomTarget - (Vector2)transform.position).normalized;
            }
        }

        // Flip sprite
        if (directionToPlayer.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (directionToPlayer.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // Attack if in range
        if (distanceToPlayer <= attackRange && canAttack)
            StartCoroutine(PerformAttack());
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isKnockedBack)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, movement * moveSpeed, 0.2f);

            // Force hovering above ground
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, hoverHeight + 0.1f, groundLayer);
            if (hit.collider != null)
            {
                float targetY = hit.point.y + hoverHeight;
                if (transform.position.y < targetY)
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }
    }

    private void PickRandomTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * randomMoveRadius;
        randomTarget = (Vector2)transform.position + randomOffset;
    }

    private IEnumerator PerformAttack()
    {
        if (player == null) yield break;

        canAttack = false;
        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead)
                playerHealth.TakeDamage(attackDamage, transform);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator?.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        if (currentHealth <= 0)
            Die(attacker);
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

    private void Die(Transform attacker = null)
    {
        if (isDead) return;

        isDead = true;
        animator?.SetBool("isDead", true);

        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        movement = Vector2.zero;

        if (attacker != null)
        {
            PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.AddMana(manaReward);
        }

        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        float deathAnimLength = 1f;

        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
                deathAnimLength = clipInfo[0].clip.length;
        }

        yield return new WaitForSeconds(deathAnimLength + 1f);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Random movement radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, randomMoveRadius);

        // Hover height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * hoverHeight);

        // Player detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
