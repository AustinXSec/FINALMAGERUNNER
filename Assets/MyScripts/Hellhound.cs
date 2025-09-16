using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Hellhound : MonoBehaviour, IDamageable
{
    public enum State { Idle, AssistToSnake, ChargeHorizontal }

    [Header("Stats")]
    public int maxHealth = 1200;
    private int currentHealth;

    [Header("Movement")]
    public float chaseSpeed = 4f;            // normal follow speed
    public float chargeSpeed = 10f;          // fast horizontal charge speed
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.25f;

    [Header("Assist")]
    public bool enableAssist = true;
    public float assistRange = 12f;          // how far the Hound considers snakes
    public float touchDistance = 0.6f;       // distance to trigger lift
    public float assistCooldown = 1.2f;      // cooldown after assist
    private float assistTimer = 0f;

    [Header("Charge")]
    public float chargeDelay = 0.2f;         // small delay before charging when player on ground
    public float chargeDuration = 8f;        // how long charge persists (treat as long run)
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("Attack")]
    public float biteRange = 1.5f;
    public int biteDamage = 50;
    public LayerMask playerLayer;

    [Header("Knockback on hit")]
    public float knockbackHorizontal = 6f;   // horizontal component of knockback applied to player
    public float knockbackVertical = 12f;    // vertical (high) knockback applied to player

    [Header("References")]
    public Transform player;
    public Animator animator;
    private Rigidbody2D rb;

    [Header("Other")]
    public AudioSource audioSource;

    [Header("Tuning")]
    public float playerAboveThreshold = 1.0f; // how much higher player must be than snake for assist

    // state
    private State state = State.Idle;
    private Snake targetSnake = null;
    private bool isAttacking = false;
    private bool isBursting = false;
    private float burstCooldown = 3f;
    private float burstTimer = 0f;
    private bool isDead = false;
    public bool IsDead => isDead;

    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true; // keep upright
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // timers
        if (assistTimer > 0f) assistTimer -= Time.deltaTime;
        if (chargeTimer > 0f) chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0f && isCharging)
        {
            EndChargeImmediate();
        }

        // If player is on the ground -> always focus player (charge/follow), no assist
        if (PlayerOnGround())
        {
            // abort assist if in progress
            if (state == State.AssistToSnake)
            {
                targetSnake = null;
                state = State.ChargeHorizontal;
            }
        }
        else
        {
            // if player not on ground, consider assist if conditions met
            if (enableAssist && state != State.AssistToSnake && assistTimer <= 0f)
            {
                Snake nearby = FindNearestSnake();
                // assign target only if snake exists and player is higher than that snake by threshold
                if (nearby != null && PlayerHigherThanSnake(nearby))
                {
                    float d = Vector2.Distance(transform.position, nearby.transform.position);
                    if (d <= assistRange)
                        targetSnake = nearby;
                }
            }
        }

        // State machine
        switch (state)
        {
            case State.Idle:
                if (PlayerOnGround())
                {
                    if (!isCharging && !isAttacking)
                        StartCoroutine(DelayThenCharge());
                }
                else
                {
                    if (targetSnake != null && PlayerHigherThanSnake(targetSnake) && assistTimer <= 0f)
                        state = State.AssistToSnake;
                    else
                        NormalFollow();
                }

                if (animator != null && Mathf.Abs(rb.velocity.x) > 0.01f)
                    animator.SetInteger("AnimState", 1);
                else if (animator != null)
                    animator.SetInteger("AnimState", 0);

                if (!isAttacking && Vector2.Distance(transform.position, player.position) <= biteRange)
                    StartCoroutine(BiteAttack());
                HandleBurst();
                break;

            case State.AssistToSnake:
                if (targetSnake == null)
                {
                    state = State.Idle;
                    break;
                }

                if (PlayerOnGround())
                {
                    targetSnake = null;
                    state = State.ChargeHorizontal;
                    break;
                }

                AssistBehaviour();
                break;

            case State.ChargeHorizontal:
                if (!isCharging)
                    StartCharge();

                if (animator != null) animator.SetInteger("AnimState", 1);
                break;
        }
    }

    // ----------------------
    // Behavior helpers
    // ----------------------
    void NormalFollow()
    {
        if (isAttacking || isBursting || isCharging) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * chaseSpeed, rb.velocity.y);

        if (animator != null)
            animator.SetInteger("AnimState", 1);
        FlipTowardsTarget(player.position);
    }

    void AssistBehaviour()
    {
        Vector2 toSnake = targetSnake.transform.position - transform.position;
        float distToSnake = toSnake.magnitude;

        float stopDistance = Mathf.Max(0.12f, touchDistance * 0.5f);
        if (Mathf.Abs(toSnake.x) > stopDistance)
        {
            rb.velocity = new Vector2(Mathf.Sign(toSnake.x) * chaseSpeed, rb.velocity.y);
            if (animator != null) animator.SetInteger("AnimState", 1);
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator != null) animator.SetInteger("AnimState", 0);
        }

        FlipTowardsTarget(targetSnake.transform.position);

        if (distToSnake <= touchDistance + 0.05f && targetSnake.IsGrounded() && PlayerHigherThanSnake(targetSnake) && assistTimer <= 0f)
        {
            Vector2 playerDir = (player.position - targetSnake.transform.position).normalized;
            float horizontal = playerDir.x * targetSnake.chaseSpeed * 1.5f;
            float vertical = targetSnake.jumpForce;

            targetSnake.ReceiveLift(new Vector2(horizontal, vertical));

            assistTimer = assistCooldown;
            targetSnake = null;

            if (animator != null)
            {
                animator.SetTrigger("Assist");
                animator.SetTrigger("Attack");
            }
            if (audioSource != null) audioSource.Play();

            state = State.ChargeHorizontal;
            StartCharge();
        }
    }

    Snake FindNearestSnake()
    {
        Snake[] snakes = FindObjectsOfType<Snake>();
        Snake nearest = null;
        float nearestDist = float.MaxValue;
        foreach (Snake s in snakes)
        {
            if (s == null) continue;
            float d = Vector2.Distance(transform.position, s.transform.position);
            if (d < nearestDist && d <= assistRange)
            {
                nearest = s;
                nearestDist = d;
            }
        }
        return nearest;
    }

    bool PlayerHigherThanSnake(Snake s)
    {
        if (s == null || player == null) return false;
        return player.position.y > s.transform.position.y + playerAboveThreshold;
    }

    bool PlayerOnGround()
    {
        return player.position.y <= transform.position.y + 0.5f;
    }

    IEnumerator DelayThenCharge()
    {
        if (isCharging || state == State.AssistToSnake) yield break;

        yield return new WaitForSeconds(chargeDelay);

        if (PlayerOnGround())
        {
            state = State.ChargeHorizontal;
            StartCharge();
        }
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTimer = chargeDuration;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dir * chargeSpeed, rb.velocity.y);
        FlipTowardsChargeDirection(dir);
        if (animator != null) animator.SetInteger("AnimState", 1);
        if (animator != null) animator.SetTrigger("Burst");
    }

    void EndChargeImmediate()
    {
        isCharging = false;
        state = State.Idle;
        // immediately stop horizontal movement and switch to idle anim
        rb.velocity = new Vector2(0f, rb.velocity.y);
        if (animator != null) animator.SetInteger("AnimState", 0);
        // reset timers that might retrigger movement
        chargeTimer = 0f;
    }

    void StartChargeForcedDirection(float dir)
    {
        // helper to start charging in a given direction (not used by default but available)
        isCharging = true;
        chargeTimer = chargeDuration;
        rb.velocity = new Vector2(dir * chargeSpeed, rb.velocity.y);
        FlipTowardsChargeDirection(dir);
        if (animator != null) animator.SetInteger("AnimState", 1);
    }

    void HandleBurst()
    {
        if (!isBursting && !isAttacking)
        {
            burstTimer += Time.deltaTime;
            if (burstTimer >= burstCooldown)
            {
                if (Random.value < 0.25f)
                    StartCoroutine(BurstTowardsPlayer());
                burstTimer = 0f;
            }
        }
    }

    IEnumerator BiteAttack()
    {
        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.2f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, biteRange, playerLayer);
        foreach (Collider2D col in hits)
        {
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage, transform);
                ApplyKnockbackToPlayerCollider(col);
            }
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    IEnumerator BurstTowardsPlayer()
    {
        isBursting = true;

        float burstDuration = 0.5f;
        float burstSpeed = chaseSpeed * 2.5f;
        float timer = 0;

        if (animator != null) animator.SetTrigger("Burst");

        while (timer < burstDuration)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(dir.x * burstSpeed, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        isBursting = false;
    }

    // Always deal damage on contact (including during charge) and apply knockback
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(biteDamage, transform);
            ApplyKnockbackToPlayerCollider(other);
            if (animator != null) animator.SetTrigger("Attack"); // ensure attack anim plays on hit
        }
    }

    // Try to apply knockback by setting player's Rigidbody2D velocity
    private void ApplyKnockbackToPlayerCollider(Collider2D playerCol)
    {
        if (playerCol == null) return;
        Rigidbody2D playerRb = playerCol.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        // knockback direction is away from hellhound horizontally, big vertical upward force
        Vector2 dir = (playerRb.transform.position - transform.position).normalized;
        float signX = Mathf.Sign(dir.x);
        playerRb.velocity = new Vector2(signX * knockbackHorizontal, knockbackVertical);
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (animator != null) animator.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDir = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir));
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.AddMana(10);

        if (currentHealth <= 0) Die();
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

        if (animator != null) animator.SetBool("IsDead", true);
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

        yield return new WaitForSeconds(deathAnimLength);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    void FlipTowardsTarget(Vector3 targetPos)
    {
        float diffX = targetPos.x - transform.position.x;
        bool facingRight = transform.localScale.x > 0;
        if (diffX > 0.1f && !facingRight) Flip();
        else if (diffX < -0.1f && facingRight) Flip();
    }

    void FlipTowardsChargeDirection(float dir)
    {
        if (dir > 0 && transform.localScale.x < 0) Flip();
        else if (dir < 0 && transform.localScale.x > 0) Flip();
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, biteRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, assistRange);
    }
}
