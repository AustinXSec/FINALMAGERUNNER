using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float spawnInterval = 3f;  // time between drops
    public int arrowsPerDrop = 3;     // number of arrows per spawn
    public float spread = 1f;         // horizontal spacing between arrows

    private void Start()
    {
        // Spawns forever in cycles
        InvokeRepeating(nameof(SpawnArrows), 2f, spawnInterval);
    }

    void SpawnArrows()
    {
        for (int i = 0; i < arrowsPerDrop; i++)
        {
            Vector3 pos = transform.position + new Vector3(i * spread, 0f, 0f);
            Instantiate(arrowPrefab, pos, Quaternion.identity);
        }
    }
}
