using UnityEngine;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 6.0f;
    [SerializeField] float m_rollForce = 10.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    [Header("Better Jump Physics")]
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2.0f;
    [SerializeField] float maxJumpHeight = 3.0f;

    [Header("Player Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds; // 3 random hurt sounds
    [Range(0f, 1f)] public float hurtVolume = 0.5f;
    public AudioClip deathSound; // 1 death sound
    [Range(0f, 1f)] public float deathVolume = 1f;
    public AudioClip jumpSound; // jump sound
    [Range(0f, 1f)] public float jumpVolume = 0.7f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;

    private bool m_grounded = false;
    private bool m_rolling = false;
    public int m_facingDirection = 1;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private float jumpStartY;

    public int FacingDirection => m_facingDirection;
    private int originalLayer; // For roll/dodge layer

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        // Roll timer
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

        // Ground detection
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", true);
        }

        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
        }

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

        // Block
        if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.LeftShift) && !m_rolling)
        {
            m_rolling = true;
            m_rollCurrentTime = 0.0f;
            gameObject.layer = LayerMask.NameToLayer("PlayerRolling");
            m_animator.SetTrigger("Roll");
        }

        // Jump
        if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            jumpStartY = transform.position.y;
            m_groundSensor.Disable(0.2f);

            // Play jump sound
            if (audioSource != null && jumpSound != null)
                audioSource.PlayOneShot(jumpSound, jumpVolume);
        }

        // Better jump physics
        if (m_body2d.velocity.y > 0)
        {
            if (Input.GetKey("space"))
            {
                if (transform.position.y - jumpStartY > maxJumpHeight)
                    m_body2d.velocity = new Vector2(m_body2d.velocity.x, 0);
            }
            else
            {
                m_body2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
        else if (m_body2d.velocity.y < 0)
        {
            m_body2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // Run / Idle
        if (!m_rolling)
        {
            if (Mathf.Abs(inputX) > Mathf.Epsilon)
            {
                m_delayToIdle = 0.05f;
                m_animator.SetInteger("AnimState", 1);
            }
            else
            {
                m_delayToIdle -= Time.deltaTime;
                if (m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
            }
        }
    }

    // Call these from PlayerHealth or animation events
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

    void AE_SlideDust()
    {
        // Placeholder for slide dust effect
    }
}
