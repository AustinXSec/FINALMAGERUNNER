using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float lifeAfterGroundHit = 0.1f; 

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Keep arrow pointing down no matter what
        transform.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If it hits the floor, despawn
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject, lifeAfterGroundHit);
        }

        // Damage player if hit, but do not despawn yet
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(20);
            }
        }
    }
}
