using System.Collections;
using UnityEngine;

public class ShroomSpawner : MonoBehaviour
{
    [Header("Enemy & Spawning")]
    public GameObject shroomPrefab;
    public float spawnInterval = 5f;
    public int maxEnemies = 3;
    public float minDistanceFromPlayer = 3f; // minimum horizontal distance from player
    public float playerSpawnOffset = 5f;     // max offset from player when spawning relative

    [Header("Spawn Area")]
    public float minX = -10f;
    public float maxX = 10f;
    public float groundY = 0f;

    private int currentEnemies = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentEnemies < maxEnemies)
                SpawnEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (shroomPrefab == null) return;

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        Vector2 spawnPos;
        int attempts = 0;

        bool spawnNearPlayer = Random.value > 0.5f; // 50% chance to spawn relative to player

        do
        {
            float x;

            if (spawnNearPlayer)
            {
                // Spawn relative to player but not too close
                float offset = Random.Range(minDistanceFromPlayer, playerSpawnOffset);
                if (Random.value > 0.5f) offset *= -1f; // left or right
                x = Mathf.Clamp(player.position.x + offset, minX, maxX);
            }
            else
            {
                // Spawn randomly anywhere in the map
                x = Random.Range(minX, maxX);
            }

            spawnPos = new Vector2(x, groundY);
            attempts++;

        } while (player != null && Vector2.Distance(spawnPos, player.position) < minDistanceFromPlayer && attempts < 10);

        GameObject enemy = Instantiate(shroomPrefab, spawnPos, Quaternion.identity);
        currentEnemies++;

        StartCoroutine(TrackEnemy(enemy));
    }

    IEnumerator TrackEnemy(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        while (enemyScript != null && !enemyScript.isDead)
            yield return null;

        currentEnemies = Mathf.Max(0, currentEnemies - 1);
    }
}
