using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance;

    [Header("References")]
    public Animator animator;
    public HeroKnight heroKnight;
    public Transform firePoint;
    public GameObject[] slashPrefabs;

    [Header("Attack Settings")]
    public float attackRange = 1.0f;
    public LayerMask enemyLayers;
    public int attackDamage = 40;

    [Header("Attack Timing")]
    public float attackDuration = 0.25f;
    public int hitFrames = 5;

    [Header("Attack Sounds")]
    public AudioClip[] attackSounds;
    public AudioClip specialAttackSound;
    [Range(0f,1f)] public float attackVolume = 1f;
    [Range(0f,1f)] public float specialVolume = 1f;
    public AudioSource audioSource;

    [Header("Special Attack")]
    public float spreadAngle = 15f;
    [HideInInspector]
    public bool specialActive = false;

    [Header("Mana Glow Particle Trail")]
    public ParticleSystem manaGlowParticles;
    public Vector3 particleOffset = new Vector3(0,1f,0); // height above player

    private int currentAttack = 0;
    private bool isAttacking = false;
    private bool attackQueued = false;

    void Awake()
    {
        Instance = this;

        // Stop particles at start
        if (manaGlowParticles != null)
            manaGlowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isAttacking)
                attackQueued = true;
            else
                StartCoroutine(PerformAttack());
        }

        // Make particles follow the player as a trail
        if (manaGlowParticles != null && specialActive)
        {
            manaGlowParticles.transform.position = transform.position + particleOffset;
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        attackQueued = false;

        currentAttack++;
        if (currentAttack > 3) currentAttack = 1;

        animator.SetTrigger("Attack" + currentAttack);

        // Play appropriate attack sound
        if (audioSource != null)
        {
            if (specialActive && specialAttackSound != null)
                audioSource.PlayOneShot(specialAttackSound, specialVolume);
            else if (attackSounds.Length > 0)
            {
                AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(clip, attackVolume);
            }
        }

        float interval = attackDuration / hitFrames;
        for (int i = 0; i < hitFrames; i++)
        {
            DealDamage();
            yield return new WaitForSeconds(interval);
        }

        isAttacking = false;

        if (attackQueued)
            StartCoroutine(PerformAttack());
    }

    void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(attackDamage, transform);
        }

        if (specialActive)
            ShootSlashBeam(currentAttack);
    }

    public void EnableSpecialAttack()
    {
        if (specialActive) return; // already active

        specialActive = true;

        // Play particles
        if (manaGlowParticles != null)
        {
            manaGlowParticles.transform.position = transform.position + particleOffset;
            var main = manaGlowParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // trail effect
            manaGlowParticles.Play();
        }

        StartCoroutine(SpecialTimer());
    }

    private IEnumerator SpecialTimer()
    {
        float duration = 30f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        specialActive = false;

        // Stop particles smoothly
        if (manaGlowParticles != null)
            manaGlowParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.ResetMana();
    }

    void ShootSlashBeam(int attackIndex)
    {
        if (slashPrefabs.Length < 3 || firePoint == null) return;

        GameObject slash = slashPrefabs[attackIndex - 1];
        int beamCount = 3;
        float angleStep = spreadAngle / (beamCount - 1);
        float startAngle = -spreadAngle / 2f;

        int facing = heroKnight.m_facingDirection;

        for (int i = 0; i < beamCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = (facing == 1) ?
                firePoint.rotation * Quaternion.Euler(0, 0, angle) :
                firePoint.rotation * Quaternion.Euler(0, 180, -angle);

            GameObject proj = Instantiate(slash, firePoint.position, rotation);

            Vector3 scale = proj.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            proj.transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
