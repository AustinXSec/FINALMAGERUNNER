using UnityEngine;

public class PotionGroundSnap : MonoBehaviour
{
    public LayerMask groundLayer;
    public float checkDistance = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, checkDistance, groundLayer);
        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + transform.localScale.y / 2f, transform.position.z);
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            Destroy(this);
        }
    }
}
