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

    // Use OnTriggerStay2D instead of OnTriggerEnter2D
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!cutsceneTriggered && other.CompareTag("Player"))
        {
            cutsceneTriggered = true;
            StartCoroutine(StartCutscene());
        }
    }

private IEnumerator StartCutscene()
{
    // ----- STOP PLAYER INPUT -----
    if (playerController != null)
        playerController.enabled = false;

    Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
    if (playerRb != null)
    {
        // Stop horizontal movement only
        playerRb.velocity = new Vector2(0f, playerRb.velocity.y);
        // Do NOT set isKinematic = true, keep gravity so player falls
    }

    // ----- FORCE PLAYER TO IDLE ANIMATION -----
    Animator playerAnimator = player.GetComponent<Animator>();
    if (playerAnimator != null)
    {
        playerAnimator.SetInteger("AnimState", 0); // idle
        playerAnimator.ResetTrigger("Jump");
        playerAnimator.ResetTrigger("Roll");
        playerAnimator.SetBool("IdleBlock", false);
        // Do NOT set Grounded = true here, let gravity do its job
    }

    // ----- NPC RUNS TO PLAYER -----
    npcAnimator.SetBool("isRunning", true);

    while (Vector2.Distance(npc.transform.position, player.position) > stopDistance)
    {
        Vector2 target = new Vector2(player.position.x, npc.transform.position.y);

        FlipTowardsPlayer();

        npc.transform.position = Vector2.MoveTowards(
            npc.transform.position,
            target,
            npcSpeed * Time.deltaTime
        );

        // Freeze player's horizontal movement while NPC approaches
        if (playerRb != null)
            playerRb.velocity = new Vector2(0f, playerRb.velocity.y);

        yield return null;
    }

    // Stop NPC
    npcAnimator.SetBool("isRunning", false);
    npcAnimator.SetTrigger("Idle");

    FlipTowardsPlayer();

    // Play dialogue / voice audio
    if (dialogueAudio != null)
    {
        dialogueAudio.Play();
        yield return new WaitForSeconds(dialogueAudio.clip.length);
    }
    else
    {
        yield return new WaitForSeconds(20f); // fallback
    }

    // Re-enable player
    if (playerController != null)
        playerController.enabled = true;

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
