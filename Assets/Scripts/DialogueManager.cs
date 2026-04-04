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
    public Image portraitImage;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

    private DialogueLine[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private System.Action onDialogueComplete;

    Coroutine typingCoroutine;

    void Awake() => Instance = this;

    void Start()
    {
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
    }

    public void PlaySequence(DialogueData data, string sequenceID, System.Action onComplete = null)
    {
        // Interrupt safely
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        StopAllCoroutines();

        isTyping = false;
        dialogueActive = false;
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
        }

        if (data == null)
        {
            // Defer so we're never calling onComplete mid-stack
            StartCoroutine(DeferredComplete(onComplete));
            return;
        }

        DialogueSequence seq = data.GetSequence(sequenceID);

        if (seq == null || seq.lines.Length == 0)
        {
            // Defer so we're never calling onComplete mid-stack
            StartCoroutine(DeferredComplete(onComplete));
            return;
        }

        currentLines = seq.lines;
        currentLineIndex = 0;
        onDialogueComplete = onComplete;
        dialogueActive = true;

        dialogueBox.SetActive(true);
        nextArrow.SetActive(false);

        ShowLine(currentLineIndex);
    }

    IEnumerator DeferredComplete(System.Action onComplete)
    {
        yield return null; // wait one frame
        onComplete?.Invoke();
    }

    void ShowLine(int index)
    {
        if (portraitImage != null)
        {
            if (currentLines[index].portrait != null)
            {
                portraitImage.sprite = currentLines[index].portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[index].line));
    }

    public void OnNextPressed()
    {
        if (!dialogueActive) return;

        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentLines[currentLineIndex].line;
            isTyping = false;
            nextArrow.SetActive(true);
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
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
    public void ForceClose()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        StopAllCoroutines();

        isTyping = false;
        dialogueActive = false;
        dialogueBox.SetActive(false);
        nextArrow.SetActive(false);
        currentLines = null;

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
        }
    }
}