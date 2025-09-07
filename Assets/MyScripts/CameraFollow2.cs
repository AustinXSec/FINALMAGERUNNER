using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;       // Drag your player here

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 0, -10);  // Camera offset from player
    public float smoothSpeed = 0.125f;               // Smooth follow speed
    public Camera cam;                               // Reference to the Camera (optional)

    [Header("Tilemap")]
    public Tilemap tilemap;     // Drag your Tilemap here

    private float camHalfHeight;
    private float camHalfWidth;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        // Use main camera if not assigned
        if (cam == null) cam = Camera.main;

        // Orthographic camera half-size
        camHalfHeight = cam.orthographicSize;                 // vertical half-height
        camHalfWidth = camHalfHeight * cam.aspect;           // horizontal half-width

        // Get tilemap bounds
        minBounds = new Vector2(tilemap.cellBounds.xMin, tilemap.cellBounds.yMin);
        maxBounds = new Vector2(tilemap.cellBounds.xMax, tilemap.cellBounds.yMax);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Desired camera position
        Vector3 desiredPos = player.position + offset;

        // Clamp to world bounds
        float clampedX = Mathf.Clamp(desiredPos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        float clampedY = Mathf.Clamp(desiredPos.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

        Vector3 clampedPos = new Vector3(clampedX, clampedY, desiredPos.z);

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, clampedPos, smoothSpeed);
    }
}
