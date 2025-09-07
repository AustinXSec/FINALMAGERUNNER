using UnityEngine;

public class Shroomy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform frontCheck;
    public float frontCheckDistance = 0.5f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Attack")]
    public int attackDamage = 10;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector3 originalScale;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Vector2 direction = player.position - transform.position;
        rb.velocity = new Vector2(Mathf.Sign(direction.x) * moveSpeed, rb.velocity.y);

        // Flip sprite
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // Jump over obstacles
        RaycastHit2D hit = Physics2D.Raycast(frontCheck.position, Vector2.right * Mathf.Sign(transform.localScale.x), frontCheckDistance, groundLayer);
        if (hit.collider != null && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                // Deal damage and apply knockback
                playerHealth.TakeDamage(attackDamage, transform);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (frontCheck != null)
            Gizmos.DrawLine(frontCheck.position, frontCheck.position + Vector3.right * frontCheckDistance * Mathf.Sign(transform.localScale.x));
    }
}
