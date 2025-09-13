using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SoulBeam : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage = 30;
    private float speed = 10f;
    private float lifetime = 3f;
    private GameObject owner;         // used to prevent friendly-fire (optional)
    public bool grantsMana = false;   // set true if beam should give mana on hit
    public LayerMask hitLayers;       // which layers the beam can hit (set in prefab)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Make sure Rigidbody2D is set to Kinematic if you don't want physics forces
    }

    public void Initialize(Vector2 direction, int damageValue, float speedValue, float lifeSeconds, GameObject ownerObj)
    {
        damage = damageValue;
        speed = speedValue;
        lifetime = lifeSeconds;
        owner = ownerObj;

        rb.velocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ignore owner
        if (collision.gameObject == owner) return;

        // Optionally check layer mask
        if (hitLayers != (hitLayers | (1 << collision.gameObject.layer)))
            return;

        // Player hit
        PlayerHealth ph = collision.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage, owner != null ? owner.transform : null);
            if (grantsMana) ph.AddMana(1);
            Destroy(gameObject);
            return;
        }

        // If it hits anything else (e.g., walls), destroy (optional)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
