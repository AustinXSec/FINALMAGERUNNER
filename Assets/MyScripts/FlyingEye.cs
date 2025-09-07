using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEye : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float verticalSpeed = 2f;
    public float stoppingDistance = 0.5f;

    [Header("Obstacle Detection")]
    public Transform frontCheck;
    public float frontCheckDistance = 0.5f;
    public float obstacleCheckHeight = 1.5f;
    public int obstacleCheckSteps = 3;
    public LayerMask obstacleLayer;

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

    [Header("Mana Reward")]
    public int manaReward = 20;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 originalScale;
    private Vector2 movement;
    private bool isKnockedBack = false;
    public bool isDead { get; private set; } = false;
    private bool canAttack = true;

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

        movement = (distance > stoppingDistance) ? direction.normalized : Vector2.zero;

        // Flip sprite
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        ObstacleDetection();

        if (distance <= attackRange && canAttack)
            StartCoroutine(PerformAttack());
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isKnockedBack)
        {
            Vector2 targetVelocity = new Vector2(movement.x * moveSpeed, movement.y * verticalSpeed);
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.2f);
        }
    }

    private void ObstacleDetection()
    {
        if (frontCheck == null) return;

        float stepHeight = obstacleCheckHeight / (obstacleCheckSteps - 1);
        bool pathClear = false;

        for (int i = 0; i < obstacleCheckSteps; i++)
        {
            Vector2 rayOrigin = frontCheck.position + Vector3.up * (i * stepHeight);
            Vector2 rayDir = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, frontCheckDistance, obstacleLayer);

            if (hit.collider == null)
            {
                movement.y = Mathf.Lerp(movement.y, verticalSpeed * (i * stepHeight / obstacleCheckHeight), 0.2f);
                pathClear = true;
                break;
            }
        }

        if (!pathClear)
        {
            movement.y = verticalSpeed;
            movement.x = Mathf.Sign(transform.localScale.x) * moveSpeed;
        }

        if (movement.y > 0 && pathClear)
            movement.y = Mathf.Lerp(movement.y, 0f, 0.05f);
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
        col.isTrigger = true;
        movement = Vector2.zero;

        // Give mana to player if attacker exists
        if (attacker != null)
        {
            PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.AddMana(manaReward);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (frontCheck != null)
        {
            Gizmos.color = Color.red;
            float stepHeight = obstacleCheckHeight / (obstacleCheckSteps - 1);
            for (int i = 0; i < obstacleCheckSteps; i++)
            {
                Vector2 origin = frontCheck.position + Vector3.up * (i * stepHeight);
                Vector2 dir = new Vector2(Mathf.Sign(transform.localScale.x), 0f) * frontCheckDistance;
                Gizmos.DrawLine(origin, origin + dir);
            }
        }

        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
