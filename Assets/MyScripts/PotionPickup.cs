using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    [Tooltip("How much health to restore")]
    public int healAmount = 25;

    [Tooltip("Optional pickup sound")]
    public AudioClip pickupSfx;
    [Range(0f,1f)]
    public float sfxVolume = 1f;

    // optional: if you want to prevent re-pickup during the same frame
    bool picked = false;

    void Awake()
    {
        // Ensure collider is trigger and we have a kinematic Rigidbody2D
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        // Find PlayerHealth component on whatever touched the potion
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            picked = true;

            // Heal the player
            ph.Heal(healAmount);

            // Play SFX at camera so sound is audible
            if (pickupSfx != null)
            {
                if (Camera.main != null)
                    AudioSource.PlayClipAtPoint(pickupSfx, Camera.main.transform.position, sfxVolume);
                else
                    AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);
            }

            // Optionally spawn pickup VFX here

            // Destroy the potion object
            Destroy(gameObject);
        }
    }
}
