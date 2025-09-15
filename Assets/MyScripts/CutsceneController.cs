using UnityEngine;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float walkSpeed = 2f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HeroKnight heroKnight;

    private void Start()
    {
        // Get components from the player
        anim = player.GetComponent<Animator>();
        spriteRenderer = player.GetComponent<SpriteRenderer>();
        heroKnight = player.GetComponent<HeroKnight>();

        // Disable player input permanently
        if (heroKnight != null)
            heroKnight.enabled = false;

        // Start the cutscene
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        // Walk continuously to exitPoint
        SetRunAnimation();
        FaceTarget(exitPoint.position);

        while (Vector3.Distance(player.position, exitPoint.position) > 0.01f)
        {
            player.position = Vector3.MoveTowards(player.position, exitPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // Stop running at the end
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
