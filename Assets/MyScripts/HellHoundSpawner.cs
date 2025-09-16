using System.Collections;
using UnityEngine;

public class HellhoundSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject hellhoundPrefab;   // Drag the Hellhound prefab here
    public Transform spawnPoint;         // Where the Hellhound will appear
    public float spawnDelay = 15f;       // Delay after Snake wakes up
    public AudioClip spawnSound;         // Sound to play when Hellhound spawns
    public AudioSource audioSource;      // AudioSource to play the sound
    public event System.Action<Hellhound> OnHellhoundSpawned;
    private bool hasSpawned = false;

    void Start()
    {
        if (hellhoundPrefab == null)
        {
            Debug.LogError("Hellhound Prefab not assigned in spawner!");
            return;
        }

        if (spawnPoint == null)
            spawnPoint = transform; // default to spawner's position

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find the Snake in the scene
        Snake snake = FindObjectOfType<Snake>();
        if (snake != null)
        {
            snake.OnSnakeWakeUp += StartSpawnCountdown; // subscribe to event
        }
    }

    void StartSpawnCountdown()
    {
        StartCoroutine(SpawnHellhoundAfterDelay());
    }

IEnumerator SpawnHellhoundAfterDelay()
{
    yield return new WaitForSeconds(spawnDelay);

    if (!hasSpawned)
    {
        GameObject obj = Instantiate(hellhoundPrefab, spawnPoint.position, Quaternion.identity);
        Hellhound h = obj.GetComponent<Hellhound>();

        if (h != null)
            OnHellhoundSpawned?.Invoke(h);

        if (spawnSound != null && audioSource != null)
            audioSource.PlayOneShot(spawnSound);

        hasSpawned = true;
    }
}
    }

