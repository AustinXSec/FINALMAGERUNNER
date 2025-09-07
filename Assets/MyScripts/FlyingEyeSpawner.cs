using System.Collections;
using UnityEngine;

public class FlyingEyeSpawner : MonoBehaviour
{
    [Header("Enemy & Spawning")]
    public GameObject flyingEyePrefab;
    public float spawnInterval = 5f;
    public int maxEnemies = 3; // max alive at once

    [Header("Spawn Area (Sky)")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = 3f; // above ground/platforms
    public float maxY = 8f;

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
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        Vector2 spawnPos = new Vector2(x, y);

        GameObject enemy = Instantiate(flyingEyePrefab, spawnPos, Quaternion.identity);
        currentEnemies++;

        // Automatically reduce count after death
        StartCoroutine(TrackEnemy(enemy));
    }

    IEnumerator TrackEnemy(GameObject enemy)
    {
        // Wait until enemy is destroyed or dead
        FlyingEye fe = enemy.GetComponent<FlyingEye>();
        while (fe != null && !fe.isDead)
            yield return null;

        currentEnemies = Mathf.Max(0, currentEnemies - 1);
    }
}
