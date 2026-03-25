using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public GameObject nextArrow;
    public Image portraitImage; // drag the portrait Image here

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

    private DialogueLine[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private System.Action onDialogueComplete;

    void Awake() => Instance = this;

    void Start()
    {
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
    }

    public void PlaySequence(DialogueData data, string sequenceID, System.Action onComplete = null)
    {
        StopAllCoroutines();
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
        isTyping = false;
        dialogueActive = false;

        Debug.Log($"[DIALOGUE] PlaySequence called. Data: {(data != null ? data.name : "NULL")} | SequenceID: {sequenceID}");

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
        }

        if (data == null)
        {
            Debug.Log("[DIALOGUE] Data is NULL — firing callback immediately.");
            onComplete?.Invoke();
            return;
        }

        DialogueSequence seq = data.GetSequence(sequenceID);

        if (seq == null || seq.lines.Length == 0)
        {
            Debug.Log($"[DIALOGUE] Sequence '{sequenceID}' not found or empty — firing callback immediately.");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[DIALOGUE] Found sequence '{sequenceID}' with {seq.lines.Length} lines.");

        currentLines = seq.lines;
        currentLineIndex = 0;
        onDialogueComplete = onComplete;
        dialogueActive = true;

        dialogueBox.SetActive(true);
        nextArrow.SetActive(false);

        ShowLine(currentLineIndex);
    }

    void ShowLine(int index)
    {
        Debug.Log($"[DIALOGUE] Showing line {index}: \"{currentLines[index].line}\"");

        if (portraitImage != null)
        {
            if (currentLines[index].portrait != null)
            {
                portraitImage.sprite = currentLines[index].portrait;
                portraitImage.gameObject.SetActive(true);
                Debug.Log($"[PORTRAIT] Showing portrait: {currentLines[index].portrait.name}");
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
                Debug.Log($"[PORTRAIT] No portrait assigned for line {index} — hiding image.");
            }
        }

        nextArrow.SetActive(true);
        StartCoroutine(TypeLine(currentLines[index].line));
    }

    public void OnNextPressed()
    {
        if (!dialogueActive) return;

        if (isTyping)
        {
            // Skip typewriter
            StopAllCoroutines();
            dialogueText.text = currentLines[currentLineIndex].line;
            isTyping = false;
            nextArrow.SetActive(true);
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            nextArrow.SetActive(false);
            ShowLine(currentLineIndex);
        }
        else
        {
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
        onDialogueComplete?.Invoke();
    }
}