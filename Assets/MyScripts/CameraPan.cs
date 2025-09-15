using UnityEngine;
using Cinemachine;

public class CameraPan : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 5f;           // How fast to pan
    public float panDistance = 3f;       // Max offset distance from player
    public bool allowHorizontal = true;  // Can pan left/right
    public bool allowVertical = true;    // Can pan up/down

    private CinemachineFramingTransposer transposer;
    private Vector3 originalOffset;

    void Start()
    {
        var vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        originalOffset = transposer.m_TrackedObjectOffset;
    }

    void Update()
    {
        Vector3 panInput = Vector3.zero;

        if (allowHorizontal)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) panInput.x -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) panInput.x += 1;
        }

        if (allowVertical)
        {
            if (Input.GetKey(KeyCode.UpArrow)) panInput.y += 1;
            if (Input.GetKey(KeyCode.DownArrow)) panInput.y -= 1;
        }

        // Clamp to max pan distance
        panInput = Vector3.ClampMagnitude(panInput, 1f); 

        Vector3 targetOffset = originalOffset + panInput * panDistance;

        // Smoothly interpolate camera offset
        transposer.m_TrackedObjectOffset = Vector3.Lerp(
            transposer.m_TrackedObjectOffset,
            targetOffset,
            Time.deltaTime * panSpeed
        );

        // If no input, reset back to original offset
        if (panInput == Vector3.zero)
        {
            transposer.m_TrackedObjectOffset = Vector3.Lerp(
                transposer.m_TrackedObjectOffset,
                originalOffset,
                Time.deltaTime * panSpeed
            );
        }
    }
}
