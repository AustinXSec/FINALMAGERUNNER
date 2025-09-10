using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpikeBall : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    [Header("Rotation Settings")]
    public float rotationSpeed = 360f; // degrees per second

    [Header("Collision Settings")]
    public float skinWidth = 0.01f; // small offset to prevent sticking

    private RaycastHit2D[] hits = new RaycastHit2D[1]; // array to receive cast results

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.velocity = Vector2.right * speed;
    }

    private void Update()
    {
        // Rotate continuously
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Predict collisions for fast movement
        DetectAndReflect();
    }

    private void DetectAndReflect()
    {
        Vector2 moveDir = rb.velocity.normalized;
        float moveDist = rb.velocity.magnitude * Time.deltaTime;

        // Cast ahead
        int hitCount = rb.Cast(moveDir, new ContactFilter2D(), hits, moveDist + skinWidth);

        if (hitCount > 0)
        {
            RaycastHit2D hit = hits[0];

            // Reflect velocity
            Vector2 newVelocity = Vector2.Reflect(rb.velocity, hit.normal).normalized * speed;
            rb.velocity = newVelocity;

            // Move ball slightly away from the surface to prevent sticking
            rb.position += hit.normal * skinWidth;
        }
    }

    // Called when the player “hits” the spikeball
    public void Deflect(Transform attacker)
    {
        Vector2 deflectDir = (transform.position - attacker.position).normalized;
        rb.velocity = deflectDir * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 newVelocity = Vector2.Reflect(rb.velocity, collision.contacts[0].normal);
        rb.velocity = newVelocity.normalized * speed;
    }
}
