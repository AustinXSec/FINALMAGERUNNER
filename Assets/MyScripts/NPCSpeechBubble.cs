using UnityEngine;
using TMPro;

public class NPCSpeechBubble : MonoBehaviour
{
    public Transform player;
    public GameObject bubbleCanvas; // assign the bubble Canvas here
    public float showDistance = 2.5f;
    public string message = "Greetings, traveler!";

    private TextMeshProUGUI bubbleText;

    private void Start()
    {
        bubbleText = bubbleCanvas.GetComponentInChildren<TextMeshProUGUI>();
        bubbleCanvas.SetActive(false); // hide at start
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= showDistance)
        {
            bubbleCanvas.SetActive(true);
            bubbleText.text = message;

            // Optional: make the bubble face the camera (if using perspective)
            bubbleCanvas.transform.rotation = Quaternion.identity;
        }
        else
        {
            bubbleCanvas.SetActive(false);
        }

        // Keep the bubble above NPC
        bubbleCanvas.transform.position = (Vector2)transform.position + Vector2.up * 1.5f;
    }
}
