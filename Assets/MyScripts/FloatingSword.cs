using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatingSword : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Movement")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.5f;
    private Vector2 floatDirection;

    [Header("Detection / Attack")]
    public float playerDetectRadius = 4f;
    public float dashAttackRange = 0.8f;
    public int dashDamage = 35;
    public float dashChargeTime = 0.5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.35f;

    [Header("References")]
    public Transform player;
    public Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    private bool isDead = false;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        floatDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isDashing)
        {
            if (distanceToPlayer <= playerDetectRadius)
            {
                StartCoroutine(ChargeThenStraightDash());
            }
            else
            {
                FloatAround();
            }
        }
    }

    void FloatAround()
    {
        Vector2 offset = floatDirection * floatSpeed * Time.deltaTime;
        float sine = Mathf.Sin(Time.time * 2f) * floatAmplitude * Time.deltaTime;
        rb.MovePosition(rb.position + offset + new Vector2(sine, sine));

        if (animator != null)
            animator.SetInteger("AnimState", 1);
    }

    IEnumerator ChargeThenStraightDash()
    {
        isDashing = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(dashChargeTime);

        Vector2 dashDir = (player.position - transform.position).normalized;
        float timer = 0f;
        HashSet<GameObject> damaged = new HashSet<GameObject>();

        while (timer < dashDuration)
        {
            if (isDead) break; // stop immediately if dead

            rb.velocity = dashDir * dashSpeed;

            // One-hit-per-dash
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashAttackRange);
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;
                GameObject go = hit.gameObject;
                if (go == gameObject) continue;
                if (damaged.Contains(go)) continue;

                PlayerHealth p = hit.GetComponent<PlayerHealth>();
                if (p != null)
                {
                    p.TakeDamage(dashDamage, transform);
                    damaged.Add(go);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        isDashing = false;
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop everything
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        isDashing = false;

        // Disable collider so it can't hit the player
        if (col != null)
            col.enabled = false;

        // Play death animation
        if (animator != null)
            animator.SetBool("IsDead", true);

        // Disable this script to stop Update / FloatAround
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
        Gizmos.DrawWireSphere(transform.position, dashAttackRange);
    }
}
