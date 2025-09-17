using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    private DialogueManager dm;
    private bool triggered = false;

    void Start()
    {
        dm = FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            if (dm != null)
                dm.StartDialogue(dialogueLines); // no lockPlayer argument
        }
    }
}
