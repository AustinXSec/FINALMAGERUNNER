using System.Collections;
using UnityEngine;

public class ShroomSpawner2 : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject shroomPrefab;      // Prefab to spawn
    public float spawnInterval = 5f;     // Time between spawns
    public int maxEnemies = 3;           // Maximum mushrooms active at once

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
                SpawnMushroom();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnMushroom()
    {
        if (shroomPrefab == null) return;

        // Spawn directly at the spawner's position
        Vector2 spawnPos = transform.position;

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
