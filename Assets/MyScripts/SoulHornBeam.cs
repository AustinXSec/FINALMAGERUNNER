using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SoulHornBeam : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage = 30;
    private float speed = 6f;          // slightly slower
    private float lifetime = 2f;       // lasts 2 seconds
    private GameObject owner;
    private Transform target;

    public bool grantsMana = false;
    public LayerMask hitLayers;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Transform targetTransform, int damageValue, float speedValue, float lifeSeconds, GameObject ownerObj)
    {
        target = targetTransform;
        damage = damageValue;
        speed = speedValue;
        lifetime = lifeSeconds;
        owner = ownerObj;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return;
        if (hitLayers != (hitLayers | (1 << collision.gameObject.layer))) return;

        PlayerHealth ph = collision.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage, owner != null ? owner.transform : null);
            if (grantsMana) ph.AddMana(1);
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
