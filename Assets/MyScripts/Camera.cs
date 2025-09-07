using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // The player to follow
    public float smoothSpeed = 0.125f; // How smoothly the camera follows
    public Vector3 offset;         // Offset from the player

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position is player position + offset
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera position
        transform.position = smoothedPosition;
    }
}
