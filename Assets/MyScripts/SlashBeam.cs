using UnityEngine;

public class SlashBeam : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;      // How fast the beam travels
    public float lifetime = 2f;    // How long before it disappears
    public int damage = 50;        // Damage per enemy

    private Rigidbody2D rb;
void Start()
{
    rb = GetComponent<Rigidbody2D>();
    if (rb == null)
        rb = gameObject.AddComponent<Rigidbody2D>();

    rb.gravityScale = 0;
    rb.isKinematic = true;

    // Move along local right
    rb.velocity = transform.right * speed;

    // 🔥 Flip sprite if facing left
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr != null && transform.localScale.x < 0)
    {
        sr.flipX = true;
    }

    Destroy(gameObject, lifetime);
}

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player
        if (other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            // Beam passes through multiple enemies
        }
    }
}
