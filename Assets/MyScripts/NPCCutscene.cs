using System.Collections;
using UnityEngine;

public class NPCCutscene : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject npc; 
    public Animator npcAnimator;
    public AudioSource dialogueAudio; // Voice audio clip
    public float npcSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Player Reference")]
    public MonoBehaviour playerController; // drag your player movement script here

    private bool cutsceneTriggered = false;
    private bool isTrackingPlayer = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!cutsceneTriggered && other.CompareTag("Player"))
        {
            cutsceneTriggered = true;
            StartCoroutine(StartCutscene());
        }
    }

    private IEnumerator StartCutscene()
    {
        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // NPC runs toward player
        npcAnimator.SetBool("isRunning", true);

        while (Vector2.Distance(npc.transform.position, player.position) > stopDistance)
        {
            Vector2 target = new Vector2(player.position.x, npc.transform.position.y);

            // Flip NPC to face the player
            FlipTowardsPlayer();

            // Move towards player
            npc.transform.position = Vector2.MoveTowards(
                npc.transform.position,
                target,
                npcSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Stop NPC
        npcAnimator.SetBool("isRunning", false);
        npcAnimator.SetTrigger("Idle");

        // Final face direction
        FlipTowardsPlayer();

        // Play dialogue / voice audio
        if (dialogueAudio != null)
        {
            dialogueAudio.Play();
            yield return new WaitForSeconds(dialogueAudio.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(20f); // fallback time
        }

        // Re-enable player movement
        if (playerController != null)
            playerController.enabled = true;

        // Start continuously tracking player after cutscene
        isTrackingPlayer = true;
    }

    private void Update()
    {
        if (isTrackingPlayer)
        {
            FlipTowardsPlayer();
        }
    }

    private void FlipTowardsPlayer()
    {
        if (player == null || npc == null) return;

        if (player.position.x < npc.transform.position.x)
            npc.transform.localScale = new Vector3(-1, 1, 1);
        else
            npc.transform.localScale = new Vector3(1, 1, 1);
    }
}
