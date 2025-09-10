using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject groundEnemyPrefab;
    public GameObject flyingEnemyPrefab;

    [Header("Spawners")]
    public Transform groundSpawnerLeft;
    public Transform groundSpawnerRight;
    public Transform flyingSpawner;

    [Header("Wave Settings")]
    public int totalWaves = 4;
    public float spawnInterval = 1.5f; // delay between each enemy spawn
    public float waveDelay = 3f;       // delay before the next wave starts

    [Header("UI")]
    public Text levelClearedText;

    [Header("Scene Loader")]
    public SceneLoader sceneLoader; // drag your SceneLoader object here

    private int currentWave = 0;

    void Start()
    {
        if (levelClearedText != null)
            levelClearedText.gameObject.SetActive(false);

        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;
            Debug.Log("Starting Wave " + currentWave);

            int enemyCount = GetEnemyCountForWave(currentWave);

            // Spawn enemies one by one
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy(currentWave);
                yield return new WaitForSeconds(spawnInterval);
            }

            // Wait before starting the next wave
            yield return new WaitForSeconds(waveDelay);
        }

        // Finished all waves
        Debug.Log("Level Cleared!");
        if (levelClearedText != null)
        {
            levelClearedText.gameObject.SetActive(true);
            levelClearedText.text = "Level Cleared!";
        }

        // Wait a bit, then load next scene
        yield return new WaitForSeconds(2f);
        if (sceneLoader != null)
        {
            sceneLoader.LoadLevel("Level_2"); // change "Level2" to your next scene’s name
        }
    }

    void SpawnEnemy(int wave)
    {
        // Decide spawn logic per wave
        if (wave == 1)
        {
            SpawnGroundEnemy();
        }
        else if (wave == 2)
        {
            if (Random.value > 0.7f) SpawnFlyingEnemy();
            else SpawnGroundEnemy();
        }
        else if (wave == 3)
        {
            if (Random.value > 0.5f) SpawnFlyingEnemy();
            else SpawnGroundEnemy();
        }
        else if (wave == 4)
        {
            if (Random.value > 0.3f) SpawnFlyingEnemy();
            else SpawnGroundEnemy();
        }
    }

    int GetEnemyCountForWave(int wave)
    {
        switch (wave)
        {
            case 1: return 3;  // easy start
            case 2: return 6;
            case 3: return 8;
            case 4: return 10; // harder
            default: return 0;
        }
    }

    void SpawnGroundEnemy()
    {
        Transform spawnPoint = (Random.value > 0.5f) ? groundSpawnerLeft : groundSpawnerRight;
        Instantiate(groundEnemyPrefab, spawnPoint.position, Quaternion.identity);
    }

    void SpawnFlyingEnemy()
    {
        Instantiate(flyingEnemyPrefab, flyingSpawner.position, Quaternion.identity);
    }
}
