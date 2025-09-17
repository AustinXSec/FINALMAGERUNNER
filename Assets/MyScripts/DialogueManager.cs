using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialogueBox;       // panel
    public Image portraitImage;          // portrait (optional)
    public TextMeshProUGUI dialogueText; // main text

    [Header("Settings")]
    public float typingSpeed = 0.03f;
    public KeyCode advanceKey = KeyCode.Space;
    public AudioSource voicePlayer;      // optional

    private Coroutine typingCoroutine;
    private DialogueLine[] lines;
    private int index;

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;

        dialogueBox.SetActive(true);
        StartCoroutine(PlayLineCoroutine());
    }

    private IEnumerator PlayLineCoroutine()
    {
        while (index < lines.Length)
        {
            DialogueLine line = lines[index];

            // portrait
            if (portraitImage != null && line.portrait != null)
                portraitImage.sprite = line.portrait;

            // start typing
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeLine(line.text));

            // play voice
            if (voicePlayer != null && line.voice != null)
                voicePlayer.PlayOneShot(line.voice);

            // wait for typing or skip
            while (typingCoroutine != null)
            {
                if (Input.GetKeyDown(advanceKey)) SkipTyping();
                yield return null;
            }

            // wait for input or auto advance
            if (line.autoAdvanceTime > 0f)
            {
                float t = 0f;
                while (t < line.autoAdvanceTime)
                {
                    if (Input.GetKeyDown(advanceKey)) break;
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (!Input.GetKeyDown(advanceKey)) yield return null;
            }

            index++;
        }

        EndDialogue();
    }

    private IEnumerator TypeLine(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = lines[index].text;
        }
    }

    private void EndDialogue()
    {
        dialogueBox.SetActive(false);
        lines = null;
        index = 0;
    }
}
