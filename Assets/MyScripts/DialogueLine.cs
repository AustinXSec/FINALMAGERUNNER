using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2,5)]
    public string text;
    public AudioClip voice;     // optional voice audio
    public Sprite portrait;     // optional portrait image
    public float autoAdvanceTime; // 0 = wait for input, >0 = auto-advance
}
