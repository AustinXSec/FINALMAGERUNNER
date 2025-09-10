using UnityEngine;

public class EnemyDeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only affect enemies
        if (collision.CompareTag("Enemy"))
        {
            // Try to get the IDamageable component (all your enemies have it)
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Kill instantly
                damageable.TakeDamage(9999);
            }
        }
    }
}
