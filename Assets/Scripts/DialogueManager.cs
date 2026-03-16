using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public GameObject nextArrow; // the arrow indicator

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f; // seconds per character

    // Internal state
    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    // Callbacks — what to do after sequence finishes
    private System.Action onDialogueComplete;

    void Awake() => Instance = this;

    void Start()
    {
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
    }

    // Call this to start any dialogue sequence
    public void PlaySequence(DialogueData data, string sequenceID, System.Action onComplete = null)
    {
        if (data == null) 
        {
            onComplete?.Invoke();
            return;
        }

        DialogueSequence seq = data.GetSequence(sequenceID);

        if (seq == null || seq.lines.Length == 0)
        {
            // No dialogue for this sequence, just fire callback
            onComplete?.Invoke();
            return;
        }

        currentLines = seq.lines;
        currentLineIndex = 0;
        onDialogueComplete = onComplete;
        dialogueActive = true;

        dialogueBox.SetActive(true);
        nextArrow.SetActive(false);

        StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    // Player clicks the arrow to advance
    public void OnNextPressed()
    {
        if (!dialogueActive) return;

        if (isTyping)
        {
            // Skip typewriter — show full line instantly
            StopAllCoroutines();
            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            nextArrow.SetActive(true);
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            nextArrow.SetActive(false);
            StartCoroutine(TypeLine(currentLines[currentLineIndex]));
        }
        else
        {
            // All lines done
            FinishDialogue();
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        nextArrow.SetActive(true);
    }

    void FinishDialogue()
    {
        dialogueActive = false;
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
        currentLines = null;
        onDialogueComplete?.Invoke(); // fire whatever comes next
    }

    public bool IsDialogueActive() => dialogueActive;
}