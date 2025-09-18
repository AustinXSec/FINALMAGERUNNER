using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HeroKnight : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 6.0f;
    [SerializeField] float m_rollForce = 10.0f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Other")]
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    [Header("Player Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)] public float hurtVolume = 0.5f;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1f;
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.7f;

    [Header("Player Health")]
    public int maxHealth = 3;

    private int currentHealth;
    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private bool m_grounded = false;
    private bool m_rolling = false;
    public int m_facingDirection = 1;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private int originalLayer;
    
    public int FacingDirection => m_facingDirection;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        originalLayer = gameObject.layer;

        if (groundCheckPoint == null)
            Debug.LogError("GroundCheckPoint not assigned in the inspector!");

        currentHealth = maxHealth; // initialize health
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleRoll();
        HandleBlock();
        HandleRunIdle();
    }

    private void FixedUpdate()
    {
        // Ground detection
        m_grounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        m_animator.SetBool("Grounded", m_grounded);

        // Moving platform support (optional)
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, 0.1f, groundLayer);
        if (hit.collider != null && hit.collider.transform != transform)
            transform.SetParent(hit.collider.transform);
        else
            transform.SetParent(null);
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");

        // Flip sprite
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);

            if (audioSource != null && jumpSound != null)
                audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    private void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !m_rolling)
        {
            m_rolling = true;
            m_rollCurrentTime = 0.0f;
            gameObject.layer = LayerMask.NameToLayer("PlayerRolling");
            m_animator.SetTrigger("Roll");
        }

        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);

            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                gameObject.layer = originalLayer;
            }
        }
    }

    private void HandleBlock()
    {
        if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
        }
    }

    private void HandleRunIdle()
    {
        if (!m_rolling)
        {
            float inputX = Input.GetAxis("Horizontal");

            if (Mathf.Abs(inputX) > Mathf.Epsilon)
            {
                m_delayToIdle = 0.05f;
                m_animator.SetInteger("AnimState", 1); // running
            }
            else
            {
                m_delayToIdle -= Time.deltaTime;
                if (m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0); // idle
            }
        }
    }

    // -------------------- Sounds --------------------
    public void PlayHurtSound()
    {
        if (audioSource != null && hurtSounds.Length > 0)
        {
            AudioClip clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
            audioSource.PlayOneShot(clip, hurtVolume);
        }
    }

    public void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound, deathVolume);
    }

    void AE_SlideDust() { }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    // -------------------- Health & Death --------------------
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        PlayHurtSound();
        m_animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        m_animator.SetTrigger("Death");
        ReloadCurrentLevel(2f); // reload after 2 seconds so death animation/sound plays
    }

    private void ReloadCurrentLevel(float delay = 0f)
    {
        if (delay > 0f)
            StartCoroutine(ReloadAfterDelay(delay));
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
