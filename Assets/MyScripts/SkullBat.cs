using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkullBat : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float dashSpeed = 8f;
    public float dashChance = 0.005f; // smaller for less frequent dash
    public float obstacleCheckDistance = 0.5f; // horizontal distance to check
    public float verticalBoost = 2f;           // vertical movement when obstacle detected
    public int obstacleCheckLayers = 3;        // number of vertical rays to check
    public float obstacleCheckHeight = 1f;     // vertical range of the check

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public Transform frontCheck; // position to check in front

    [Header("Mana Reward")]
    public int manaReward = 20;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 originalScale;
    private Vector2 movement;
    private bool isKnockedBack = false;
    public bool isDead { get; private set; } = false;
    private bool canAttack = true;
    private bool isDashing = false;

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
    }

    void Update()
    {
        if (isDead || player == null) return;

        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Decide movement
        if (!isDashing)
        {
            movement = (distance > attackRange) ? direction.normalized : Vector2.zero;

            // Multi-layer front obstacle detection
            if (frontCheck != null)
            {
                bool obstacleDetected = false;
                float step = obstacleCheckHeight / (obstacleCheckLayers - 1);

                for (int i = 0; i < obstacleCheckLayers; i++)
                {
                    Vector2 rayOrigin = frontCheck.position + Vector3.up * (i * step);
                    Vector2 rayDir = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, obstacleCheckDistance, LayerMask.GetMask("Ground"));

                    if (hit.collider != null)
                    {
                        obstacleDetected = true;
                        break; // stop checking other layers once an obstacle is found
                    }
                }

                if (obstacleDetected)
                    movement.y = verticalBoost; // move up if obstacle detected
            }
        }

        // Flip sprite
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // Random dash
        if (!isDashing && Random.value < dashChance)
            StartCoroutine(DashAttack());

        // Attack if in range
        if (distance <= attackRange && canAttack)
            StartCoroutine(PerformAttack());
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isKnockedBack && !isDashing)
            rb.velocity = Vector2.Lerp(rb.velocity, movement * moveSpeed, 0.2f);
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

    private IEnumerator DashAttack()
    {
        isDashing = true;

        Vector2 dashDirection = (player.position - transform.position).normalized;
        float dashDuration = 0.3f;
        float timer = 0f;

        animator?.SetTrigger("Dash");

        while (timer < dashDuration)
        {
            rb.velocity = dashDirection * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
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
        col.isTrigger = true;
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

    void OnDrawGizmosSelected()
    {
        if (frontCheck != null)
        {
            Gizmos.color = Color.red;
            float step = obstacleCheckHeight / (obstacleCheckLayers - 1);

            for (int i = 0; i < obstacleCheckLayers; i++)
            {
                Vector2 rayOrigin = frontCheck.position + Vector3.up * (i * step);
                Vector2 rayDir = new Vector2(Mathf.Sign(transform.localScale.x), 0f) * obstacleCheckDistance;
                Gizmos.DrawLine(rayOrigin, rayOrigin + rayDir);
            }
        }
    }
}
