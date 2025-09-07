using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;     
    public float spawnInterval = 5f;      
    public int maxEnemies = 20;           

    [Header("Spawn Settings")]
    public float spawnDistanceFromCamera = 2f;  // How far off-screen to spawn
    public LayerMask groundLayer;                // Ground layer for raycast

    private Camera mainCamera;
    private int currentEnemyCount = 0;

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                currentEnemyCount++;

                spawnInterval = Mathf.Max(1f, spawnInterval - 0.05f);
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector3 cameraPos = mainCamera.transform.position;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Only spawn on left or right
        float spawnX = 0f;
        int side = Random.Range(0, 2); // 0 = left, 1 = right

        if (side == 0)
            spawnX = cameraPos.x - cameraWidth / 2 - spawnDistanceFromCamera;
        else
            spawnX = cameraPos.x + cameraWidth / 2 + spawnDistanceFromCamera;

        // Raycast down from high above to find the ground
        float rayStartY = cameraPos.y + cameraHeight / 2 + 5f; // Start above camera
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawnX, rayStartY), Vector2.down, 50f, groundLayer);

        if (hit.collider != null)
        {
            Vector3 spawnPos = new Vector3(spawnX, hit.point.y, 0f);
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No ground detected at spawn X: " + spawnX);
        }
    }
}
