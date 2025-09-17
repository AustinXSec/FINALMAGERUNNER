using UnityEngine;

public class PlaySoundTrigger : MonoBehaviour
{
    private AudioSource audioSource;
    private bool hasTriggered = false; // ensures it triggers only once

    [Header("Optional: start audio slightly into clip (seconds)")]
    public float startOffset = 0f; // set to 0.05f if you want to skip a tiny pause

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Ensure AudioSource is ready
        if (audioSource != null)
        {
            audioSource.loop = false; // no looping
            audioSource.playOnAwake = false; // prevent auto-play
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (audioSource != null && audioSource.clip != null)
            {
                // Start audio from an offset if needed
                audioSource.time = startOffset;
                audioSource.Play();
            }

            // Optional: destroy trigger if you don't want it to remain
            // Destroy(gameObject);
        }
    }
}
