using UnityEngine;
using System.Collections;

public class CutsceneControllerWithAudio : MonoBehaviour
{
    [Header("Player & Exit")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float walkSpeed = 2f;

    [Header("NPC Voice")]
    [SerializeField] private AudioSource npcAudio;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HeroKnight heroKnight;

    private void Start()
    {
        // Get components from player
        anim = player.GetComponent<Animator>();
        spriteRenderer = player.GetComponent<SpriteRenderer>();
        heroKnight = player.GetComponent<HeroKnight>();

        // Disable player input
        if (heroKnight != null)
            heroKnight.enabled = false;

        // Start cutscene
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        // Play NPC audio if assigned
        if (npcAudio != null)
        {
            npcAudio.Play();
            yield return new WaitForSeconds(npcAudio.clip.length);
        }

        // Walk to exit point
        SetRunAnimation();
        FaceTarget(exitPoint.position);

        while (Vector3.Distance(player.position, exitPoint.position) > 0.01f)
        {
            player.position = Vector3.MoveTowards(player.position, exitPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // Stop running
        SetIdleAnimation();
        Debug.Log("Cutscene finished!");
    }

    private void SetRunAnimation()
    {
        if (anim != null)
            anim.SetInteger("AnimState", 1); // Running
    }

    private void SetIdleAnimation()
    {
        if (anim != null)
            anim.SetInteger("AnimState", 0); // Idle
    }

    private void FaceTarget(Vector3 target)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = target.x < player.position.x; // faces left if target is left
    }
}
