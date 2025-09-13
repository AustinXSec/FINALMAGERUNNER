using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Soul : MonoBehaviour, IDamageable
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
    public Transform player;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isDead = false;
    private bool facingRight = false;
    private bool isAttacking = false;

    [Header("Visual / Sprite")]
    [Tooltip("Set true if the sprite art faces right by default. If your art faces left by default, set false.")]
    public bool spriteFacesRight = false;

    [Header("Soul Beam Attack")]
    public Transform firePoint;
    public GameObject soulBeamPrefab;
    public int beamCount = 3;
    public float spreadAngle = 15f;
    public float chargeTime = 1.0f;

    [Header("Beam Settings")]
    public int beamDamage = 30;
    public float beamSpeed = 12f;
    public float beamLifetime = 3f;

    [Header("Audio")]
    public AudioClip chargeAndFireClip;
    [Range(0f,1f)] public float audioVolume = 1f;
    public AudioSource audioSource;

    [Header("Flip / Tuning")]
    public float flipDeadzone = 0.1f;

    private Vector3 savedFirePointLocalPos;

    // ===================== NEW MELEE ATTACK VARIABLES =====================
    [Header("Melee Attack")]
    public float meleeRange = 1.5f; // distance at which the soul can melee attack
    public int meleeDamage = 50;
    public float meleeCooldown = 1.5f;
    private bool canMelee = true;
    // ======================================================================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        if (firePoint != null)
            savedFirePointLocalPos = firePoint.localPosition;
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        facingRight = spriteFacesRight;  // <-- if spriteFacesRight=false, facingRight=false
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
        UpdateSpriteFlipAndFirePoint();
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= playerDetectRadius)
        {
            // Start charging & shooting when player is detected
            if (!isAttacking)
            {
                StartCoroutine(ChargeAndShootBeam());
            }

            // Optional: Move toward player while charging
            MoveTowardsPlayer();

            // ===================== MELEE ATTACK CHECK =====================
            if (distanceToPlayer <= meleeRange && canMelee)
            {
                StartCoroutine(MeleeAttack());
            }
            // ===============================================================
        }
    }

    void MoveTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        rb.velocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.velocity.y);
        FlipTowardsPlayer();
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;

        float diffX = player.position.x - transform.position.x;

        if (diffX > flipDeadzone && !facingRight)
        {
            facingRight = true;
            UpdateSpriteFlipAndFirePoint();
        }
        else if (diffX < -flipDeadzone && facingRight)
        {
            facingRight = false;
            UpdateSpriteFlipAndFirePoint();
        }
    }

    void UpdateSpriteFlipAndFirePoint()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = spriteFacesRight ? !facingRight : facingRight;

        if (firePoint != null)
        {
            float offsetX = Mathf.Abs(savedFirePointLocalPos.x);
            firePoint.localPosition = new Vector3(facingRight ? offsetX : -offsetX, savedFirePointLocalPos.y, savedFirePointLocalPos.z);
            firePoint.localRotation = Quaternion.Euler(0, 0, facingRight ? 0f : 180f);
        }
    }

    IEnumerator ChargeAndShootBeam()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Charge");

        if (audioSource != null && chargeAndFireClip != null)
            audioSource.PlayOneShot(chargeAndFireClip, audioVolume);

        yield return new WaitForSeconds(chargeTime);

        ShootSoulBeam();

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    void ShootSoulBeam()
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = beamCount > 1 ? spreadAngle / (beamCount - 1) : 0f;

        for (int i = 0; i < beamCount; i++)
        {
            float offset = startAngle + angleStep * i;
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, offset);

            GameObject beam = Instantiate(soulBeamPrefab, firePoint.position, rotation);
            SoulBeam beamComp = beam.GetComponent<SoulBeam>();
            if (beamComp != null)
            {
                Vector2 dir = rotation * Vector2.right;
                beamComp.Initialize(dir, beamDamage, beamSpeed, beamLifetime, gameObject);
            }
        }
    }

    // ===================== MELEE ATTACK COROUTINE =====================
    IEnumerator MeleeAttack()
    {
        canMelee = false;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (player != null)
        {
            // Deal melee damage if still in range
            if (Vector2.Distance(transform.position, player.position) <= meleeRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(meleeDamage);
                }
            }
        }

        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
    }
    // ===============================================================

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (animator != null) animator.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDir = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir));

            PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.AddMana(30);
        }

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
        if (animator != null) animator.SetBool("isDead", true);
        this.enabled = false;
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        float deathAnimLength = 1f;
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0) deathAnimLength = clipInfo[0].clip.length;
        }

        yield return new WaitForSeconds(deathAnimLength + 2f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        // Draw melee range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(firePoint.position, 0.05f);
        }
    }
}
