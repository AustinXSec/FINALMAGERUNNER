using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bird : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 800;
    private int currentHealth;

    [Header("Movement")]
    public float chaseSpeed = 5f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Detection")]
    public float playerDetectRadius = 4f;
    public float attackRange = 1f;
    public Transform player;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float jumpCooldown = 2f; // time between possible jumps
    private float jumpTimer = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool facingRight = true;
    private bool isAttacking = false;

    [Header("Flip Settings")]
    public float flipDeadzone = 0.1f;

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

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= playerDetectRadius)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
                HandleRandomJump();
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
            HandleRandomJump();
        }

        FlipTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        rb.velocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.velocity.y);
    }

    void HandleRandomJump()
    {
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f && IsGrounded())
        {
            if (Random.value < 0.5f) // 50% chance to jump
            {
                Vector2 jumpDirection = Vector2.up;

                // 70% chance to jump toward player if in range
                if (player != null && Vector2.Distance(transform.position, player.position) <= playerDetectRadius && Random.value < 0.7f)
                {
                    float dirX = player.position.x - transform.position.x;
                    jumpDirection = new Vector2(Mathf.Sign(dirX), 1).normalized;
                }

                rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
                jumpTimer = jumpCooldown;
            }
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
        isAttacking = true;

        if (animator != null)
        {
            string attackTrigger = Random.value < 0.5f ? "Attack1" : "Attack2";
            animator.SetTrigger(attackTrigger);
        }

        yield return new WaitForSeconds(0.3f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject == this.gameObject) continue;

            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(45, transform);
                playerHealth.AddMana(1);
            }
        }

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (animator != null)
            animator.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDir = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir));

            PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddMana(30);
            }
        }

        if (currentHealth <= 0)
            Die();
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

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
