using UnityEngine;

public class PotionSpawner : MonoBehaviour
{
    [Header("Potion Settings")]
    public GameObject potionPrefab;
    public int spawnCount = 5;
    public float horizontalRange = 5f;
    public float spawnHeight = 5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    private void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnPotion();
        }
    }

    void SpawnPotion()
    {
        // Random horizontal spawn position
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-horizontalRange, horizontalRange), spawnHeight, 0);
        GameObject potion = Instantiate(potionPrefab, spawnPos, Quaternion.identity);

        // Add Rigidbody2D if missing
        Rigidbody2D rb = potion.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = potion.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        // Start coroutine to make potion stop on ground
        potion.AddComponent<PotionFall>().Init(groundLayer, groundCheckDistance);
    }
}

public class PotionFall : MonoBehaviour
{
    private Rigidbody2D rb;
    private LayerMask groundLayer;
    private float checkDistance;

    public void Init(LayerMask groundLayer, float checkDistance)
    {
        this.groundLayer = groundLayer;
        this.checkDistance = checkDistance;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Raycast downward to detect ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, checkDistance, groundLayer);
        if (hit.collider != null)
        {
            // Snap potion to the ground
            transform.position = new Vector3(transform.position.x, hit.point.y + transform.localScale.y / 2f, transform.position.z);
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // stop physics
            Destroy(this); // remove this component
        }
    }
}
