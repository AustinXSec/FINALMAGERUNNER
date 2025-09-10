using UnityEngine;
using Cinemachine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera bossRoomCam;
    [SerializeField] private int activePriority = 20;

    [Header("Boss Room Wall")]
    [SerializeField] private GameObject bossRoomWall;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Switch camera
            bossRoomCam.Priority = activePriority;

            // Activate the invisible wall
            if (bossRoomWall != null)
                bossRoomWall.SetActive(true);

            // Disable this trigger so it doesn’t fire again
            gameObject.SetActive(false);
        }
    }
}
