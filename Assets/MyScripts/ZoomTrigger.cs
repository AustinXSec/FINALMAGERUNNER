using UnityEngine;
using Cinemachine;

public class CameraZoneSwitch : MonoBehaviour
{
    public CinemachineVirtualCamera zoomOutCam;
    public int activePriority = 20;
    private int defaultPriority;

    void Start()
    {
        defaultPriority = zoomOutCam.Priority;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            zoomOutCam.Priority = activePriority;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            zoomOutCam.Priority = defaultPriority;
    }
}
