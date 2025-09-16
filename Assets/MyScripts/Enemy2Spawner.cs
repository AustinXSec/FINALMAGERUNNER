using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2pawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;      // Only one prefab per spawner
    public float initialDelay = 15f;
    public float spawnInterval = 5f;
    public int maxEnemiesAlive = 10;
    public int enemiesPerSpawn = 1;

    [HideInInspector]
    public List<GameObject> spawnedEnemies = new List<GameObject>(); // make public for BossFightManager access

    private bool canSpawn = false;

    void Start()
    {
        // Subscribe to Snake wake-up event
        Snake snake = FindObjectOfType<Snake>();
        if (snake != null)
            snake.OnSnakeWakeUp += HandleSnakeWakeUp;
        else
            Debug.LogWarning("No Snake found in scene for EnemySpawner to listen to.");
    }

    void HandleSnakeWakeUp()
    {
        if (!canSpawn)
        {
            canSpawn = true;
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        // Wait before first spawn
        yield return new WaitForSeconds(initialDelay);

        while (canSpawn)
        {
            // Stop spawning if both bosses are dead
            Snake snake = FindObjectOfType<Snake>();
            Hellhound hellhound = FindObjectOfType<Hellhound>();

            bool snakeDead = (snake == null || snake.IsDead);
            bool hellhoundDead = (hellhound == null || hellhound.IsDead);

            if (snakeDead && hellhoundDead)
            {
                // Stop spawning and optionally hide spawner
                canSpawn = false;
                gameObject.SetActive(false);
                yield break;
            }

            // Clean up destroyed enemies from list
            spawnedEnemies.RemoveAll(e => e == null);

            if (spawnedEnemies.Count < maxEnemiesAlive)
            {
                int enemiesToSpawn = Mathf.Min(enemiesPerSpawn, maxEnemiesAlive - spawnedEnemies.Count);

                for (int i = 0; i < enemiesToSpawn; i++)
                    SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        spawnedEnemies.Add(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a small gizmo to visualize the spawn point
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
