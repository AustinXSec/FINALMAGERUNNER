using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player; // Assign the player in the inspector
    public Transform respawnPoint; // Starting point of the level

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Reset player position
            collision.transform.position = respawnPoint.position;

            // Optional: Reset velocity if using Rigidbody2D
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
}
