using UnityEngine;

public class SpikeSlab : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 20;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object has a PlayerHealth/IDamageable
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Find the contact point
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Damage only if the player hit the *top* of the spikes
                if (contact.normal.y < -0.5f) // player landed on top
                {
                    damageable.TakeDamage(damage, transform);
                    break;
                }
            }
        }
    }
}
