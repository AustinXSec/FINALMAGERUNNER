using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Goblin : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 600;
    private int currentHealth;

    [Header("Movement")]
    public float chaseSpeed = 6f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Jumping/Detection Points")]
    public Transform groundCheckPoint;   // slightly below/forward
    public Transform obstacleCheckPoint; // slightly in front
    public float checkRadius = 0.1f;     
    public LayerMask groundLayer;
    public float jumpForce = 8f;

    [Header("Detection")]
    public float playerDetectRadius = 3f; 
    public float dashAttackRange = 1f;
    public Transform player;

    [Header("Potion Drop")]
    public GameObject potionPrefab;         // Assign potion prefab
    public int minDrop = 1;                 // Minimum potions
    public int maxDrop = 3;                 // Maximum potions
    public float horizontalSpread = 1f;     // Horizontal spread
    public float dropHeight = 1f;           // Drop height
    [Range(0f, 1f)]
    public float dropChance = 0.5f;         // Chance to drop
    public LayerMask potionGroundLayer;     // Ground layer for potions

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool facingRight = true;
    private bool isDashing = false;

    [Header("Flip Settings")]
    public float flipDeadzone = 0.1f; // prevent rapid flips

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (groundCheckPoint == null)
            Debug.LogWarning("GroundCheckPoint not assigned!");
        if (obstacleCheckPoint == null)
            Debug.LogWarning("ObstacleCheckPoint not assigned!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= playerDetectRadius)
        {
            if (distanceToPlayer > dashAttackRange)
            {
                MoveTowardsPlayer(); 
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (animator != null)
                    animator.SetInteger("AnimState", 0); 
                if (!isDashing)
                    StartCoroutine(PrepareAndDash());
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (animator != null)
                animator.SetInteger("AnimState", 0);
        }

        FlipTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;

        if ((IsObstacleAhead() || !IsGroundAhead()) && IsGrounded())
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        rb.velocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.velocity.y);

        if (animator != null)
            animator.SetInteger("AnimState", 1); // Run animation
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

    IEnumerator PrepareAndDash()
    {
        isDashing = true;
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(DashAttack());
    }

    IEnumerator DashAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        string attackTrigger = Random.value < 0.5f ? "Attack1" : "Attack2";
        if (animator != null)
            animator.SetTrigger(attackTrigger);

        yield return new WaitForSeconds(0.3f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashAttackRange);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject == this.gameObject) continue;
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(35, transform);
        }

        yield return new WaitForSeconds(0.2f);
        isDashing = false;
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
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.AddMana(6);

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

        StopAllCoroutines();
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

        yield return new WaitForSeconds(deathAnimLength);

        // Drop potions before destroy
        if (potionPrefab != null && Random.value <= dropChance)
            DropPotions();

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void DropPotions()
    {
        int dropCount = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-horizontalSpread, horizontalSpread), dropHeight, 0);
            GameObject potion = Instantiate(potionPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rbPotion = potion.GetComponent<Rigidbody2D>();
            if (rbPotion == null)
                rbPotion = potion.AddComponent<Rigidbody2D>();

            rbPotion.bodyType = RigidbodyType2D.Dynamic;
            rbPotion.gravityScale = 1f;

            Collider2D colPotion = potion.GetComponent<Collider2D>();
            if (colPotion == null)
            {
                colPotion = potion.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)colPotion).radius = 0.2f;
            }

            rbPotion.AddForce(new Vector2(Random.Range(-1f, 1f), 0), ForceMode2D.Impulse);

            PotionGroundSnap snap = potion.AddComponent<PotionGroundSnap>();
            snap.groundLayer = potionGroundLayer;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashAttackRange);

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
