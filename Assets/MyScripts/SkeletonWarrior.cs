using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonWarrior : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 800;
    private int currentHealth;

    [Header("Movement")]
    public float chaseSpeed = 5f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Jumping/Detection Points")]
    public Transform groundCheckPoint;
    public Transform obstacleCheckPoint;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;
    public float jumpForce = 8f;

    [Header("Detection")]
    public float playerDetectRadius = 4f;
    public float attackRange = 1f;
    public Transform player;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool facingRight = true;
    private bool isAttacking = false;

    [Header("Flip Settings")]
    public float flipDeadzone = 0.1f;

    [Header("Shield Settings")]
    [Range(0f, 1f)] public float shieldChance = 0.3f; // 30% chance to block
    private bool isBlocking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Freeze everything while blocking
        if (isBlocking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= playerDetectRadius)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (!isAttacking)
                    StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // idle
        }

        FlipTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        if (isBlocking) return;

        float directionX = player.position.x - transform.position.x;

        if ((IsObstacleAhead() || !IsGroundAhead()) && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        rb.velocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.velocity.y);
    }

    bool IsGroundAhead()
    {
        if (groundCheckPoint == null) return false;
        return Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer) != null;
    }

    bool IsObstacleAhead()
    {
        if (obstacleCheckPoint == null) return false;
        return Physics2D.OverlapCircle(obstacleCheckPoint.position, checkRadius, groundLayer) != null;
    }

    bool IsGrounded()
    {
        if (groundCheckPoint == null) return false;
        return Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer) != null;
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;

        float diffX = player.position.x - transform.position.x;

        if (diffX > flipDeadzone && !facingRight)
            facingRight = true;
        else if (diffX < -flipDeadzone && facingRight)
            facingRight = false;

        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    IEnumerator AttackPlayer()
    {
        if (isBlocking) yield break;

        isAttacking = true;

        if (animator != null)
        {
            string attackTrigger = Random.value < 0.5f ? "Attack1" : "Attack2";
            animator.SetTrigger(attackTrigger);
        }

        yield return new WaitForSeconds(0.3f);

        if (isBlocking)
        {
            isAttacking = false;
            yield break;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject == this.gameObject) continue;

            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(45, transform);

                // Give mana to player
                playerHealth.AddMana(1);
            }
        }

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

   public void TakeDamage(int damage, Transform attacker = null)
{
    if (isDead || isBlocking) return;

    // Check for shield block first
    if (Random.value < shieldChance)
    {
        StartCoroutine(DoShieldBlock());
        return;
    }

    currentHealth -= damage;
    if (animator != null)
        animator.SetTrigger("Hurt");

    // Apply knockback
    if (attacker != null)
    {
        Vector2 knockbackDir = (transform.position - attacker.position).normalized;
        StartCoroutine(ApplyKnockback(knockbackDir));
    }

    // GIVE MANA TO PLAYER if attacker is player
    if (attacker != null)
    {
        PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.AddMana(30); // <-- fill mana when YOU attack the skeleton
        }
    }

    if (currentHealth <= 0)
        Die();
}


    private IEnumerator DoShieldBlock()
    {
        isBlocking = true;
        rb.velocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("ShieldBlock");

        float blockDuration = Random.Range(0.5f, 1.5f);
        float timer = 0f;

        while (timer < blockDuration)
        {
            rb.velocity = Vector2.zero; // freeze each frame
            timer += Time.deltaTime;
            yield return null;
        }

        isBlocking = false;
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        float timer = 0f;
        direction.y = 0f;
        direction.Normalize();

        while (timer < knockbackDuration)
        {
            rb.velocity = new Vector2(direction.x * knockbackForce, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.SetBool("IsDead", true);

        this.enabled = false;
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        float deathAnimLength = 1f;
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
                deathAnimLength = clipInfo[0].clip.length;
        }

        yield return new WaitForSeconds(deathAnimLength + 2f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }
        if (obstacleCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(obstacleCheckPoint.position, checkRadius);
        }
    }
}
