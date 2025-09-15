using System.Collections;
using UnityEngine;
using Cinemachine; // make sure this is at the top

[RequireComponent(typeof(Rigidbody2D))]
public class Snake : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 1200;
    private int currentHealth;

    [Header("Movement")]
    public float chaseSpeed = 4f;
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.25f;
    public float jumpForce = 6f;

    [Header("Detection Points")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkRadius = 0.1f;

    [Header("Cutscene Trigger")]
    public float detectRadius = 5f;
    public Vector2 detectOffset = new Vector2(2f, 0f);
    public float wakeDelay = 3f;
    private bool hasWoken = false;    // has cutscene finished?
    private bool wakingUp = false;   // waiting delay

    [Header("Boss Music (optional)")]
    public AudioClip bossMusicClip;
    public float musicFadeSeconds = 1.5f;



    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Attack")]
    public float biteRange = 1.5f;
    public Transform player;
    public int biteDamage = 50;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isAttacking = false;
    public bool IsDead => isDead;

    [Header("Flip Settings")]
    public float flipDeadzone = 0.1f;

    public delegate void SnakeWakeUpAction();
    public event SnakeWakeUpAction OnSnakeWakeUp;

    // Optional burst dash
    public bool enableBurst = true;
    private bool isBursting = false;
    private float burstCooldown = 3f;
    private float burstTimer = 0f;

    // NEW: control being lifted by ally
    private float liftCooldown = 0.6f;
    private float liftTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (groundCheckPoint == null)
            Debug.LogWarning("GroundCheckPoint not assigned!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        // --- CUTSCENE LOGIC ---
        if (!hasWoken)
        {
            DetectPlayerInFront();
            return; // block AI until woken
        }

        if (liftTimer > 0f) liftTimer -= Time.deltaTime;

        // --- NORMAL AI LOGIC ---
        if (!isAttacking && !isBursting)
            MoveTowardsPlayer();

        FlipTowardsPlayer();

        // Attack if close
        if (!isAttacking && !isBursting && Vector2.Distance(transform.position, player.position) <= biteRange)
        {
            StartCoroutine(BiteAttack());
        }

        // ORIGINAL snake auto-leap removed from autonomous decision (leave LeapToPlatform method available)
        /*
        if (!isAttacking && !isBursting && player.position.y > transform.position.y + 1f && IsGrounded())
        {
            StartCoroutine(LeapToPlatform());
        }
        */

        // Optional burst
        if (enableBurst && !isAttacking && !isBursting)
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

    void DetectPlayerInFront()
    {
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 center = (Vector2)transform.position + new Vector2(detectOffset.x * facingDir, detectOffset.y);

        Collider2D hit = Physics2D.OverlapCircle(center, detectRadius, LayerMask.GetMask("Player"));
        if (hit != null && !wakingUp)
        {
            StartCoroutine(WakeUpSequence());
        }
    }

    IEnumerator WakeUpSequence()
    {
        wakingUp = true;

        if (animator != null)
            animator.SetTrigger("WakeUp"); // optional animation

        yield return new WaitForSeconds(wakeDelay);

        // --- Play the AudioSource directly ---
        if (audioSource != null)
        {
            audioSource.Play(); // plays the clip assigned on the AudioSource component
        }

        // --- Trigger camera shake ---
        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        hasWoken = true;   // snake is now active forever
        // Example inside Snake.WakeUpSequence(), after hasWoken = true;
if (MusicManager.Instance != null && bossMusicClip != null)
{
    MusicManager.Instance.CrossfadeTo(bossMusicClip, 1.5f, true); // fade to boss music over 1.5s and loop
}

        wakingUp = false;

        OnSnakeWakeUp?.Invoke();
        
    }

    void MoveTowardsPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * chaseSpeed, rb.velocity.y);

        if (animator != null)
            animator.SetInteger("AnimState", 1); // run animation
    }

    IEnumerator BiteAttack()
    {
        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f); // attack delay

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, biteRange);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject == this.gameObject) continue;

            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(biteDamage, transform);
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    IEnumerator LeapToPlatform()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetTrigger("Leap");

        yield return new WaitForSeconds(0.5f); // wind-up

        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * chaseSpeed * 1.5f, jumpForce);

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator BurstTowardsPlayer()
    {
        isBursting = true;

        float burstDuration = 0.5f;
        float burstSpeed = chaseSpeed * 2.5f;
        float timer = 0;

        if (animator != null)
            animator.SetTrigger("Burst");

        while (timer < burstDuration)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(dir.x * burstSpeed, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        isBursting = false;
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;

        float diffX = player.position.x - transform.position.x;
        bool facingRight = transform.localScale.x > 0;

        if (diffX > flipDeadzone && !facingRight)
            Flip();
        else if (diffX < -flipDeadzone && facingRight)
            Flip();
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer) != null;
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
            playerHealth.AddMana(10);

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

        yield return new WaitForSeconds(deathAnimLength);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

public void ReceiveLift(Vector2 liftVelocity)
{
    // Only allow lift when grounded and not attacking/dead
    if (isDead || isAttacking) return;
    if (!IsGrounded()) return;
    if (liftTimer > 0f) return;

    StartCoroutine(HandleReceivedLift(liftVelocity));
}

private IEnumerator HandleReceivedLift(Vector2 liftVelocity)
{
    isAttacking = true; // prevent other actions during lift
    if (animator != null)
        animator.SetTrigger("Leap"); // play leap animation

    // short wind-up
    yield return new WaitForSeconds(0.2f);

    // Increase vertical velocity for stronger first jump
    float strongJumpY = liftVelocity.y * 1.2f; // 50% higher than original
    rb.velocity = new Vector2(liftVelocity.x, strongJumpY);

    // Wait a small fraction before applying second vertical boost
    yield return new WaitForSeconds(0.15f);

    // Apply **second jump** with increased force
    rb.velocity = new Vector2(rb.velocity.x, strongJumpY);

    // wait before resuming normal AI
    yield return new WaitForSeconds(0.4f);

    isAttacking = false;
    liftTimer = liftCooldown;
}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 center = (Vector2)transform.position + new Vector2(detectOffset.x * facingDir, detectOffset.y);
        Gizmos.DrawWireSphere(center, detectRadius);

        Gizmos.color = Color.red;
        if (groundCheckPoint != null)
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
}
