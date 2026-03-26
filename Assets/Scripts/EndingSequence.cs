using System.Collections;
using UnityEngine;
using TMPro;

// 🔹 Each final line
[System.Serializable]
public class EndingLine
{
    [TextArea(2, 4)]
    public string line;

    public float typeSpeed = 0.03f;
}

public class EndingSequence : MonoBehaviour
{
    // ───────────── DAY TITLE ─────────────
    [Header("DAY TITLE")]
    public TextMeshProUGUI dayTitleText;
    public float titleDuration = 2f;

    // ───────────── FLAVOUR TEXT ─────────────
    [Header("FLAVOUR TEXT")]
    public TextMeshProUGUI dayFlavourText;

    [TextArea(2, 5)]
    public string flavourText;

    public float flavourTypeSpeed = 0.03f;
    public float flavourStayDuration = 2f;

    // ───────────── ANIMATION PREFAB ─────────────
    [Header("ANIMATION PREFAB")]
    public GameObject animationPrefab;
    public float animationDuration = 17f;

    // ───────────── FINAL TEXT ─────────────
    [Header("FINAL TEXT")]
    public TextMeshProUGUI finalText;
    public EndingLine[] finalLines;

    // ───────────── NEXT BUTTON CONTROL ─────────────
    [Header("NEXT BUTTON")]
    public bool nextPressed = false;

    private bool isTyping = false;
    private bool skipRequested = false;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    // 🔘 Hook this to your Next button
    public void OnNextPressed()
    {
        nextPressed = true;

        if (isTyping)
        {
            skipRequested = true; // instantly finish typing
        }
    }

    IEnumerator PlaySequence()
    {
        // ───────────── TITLE ─────────────
        if (dayTitleText != null)
        {
            dayTitleText.gameObject.SetActive(true);
            yield return new WaitForSeconds(titleDuration);
            dayTitleText.gameObject.SetActive(false);
        }

        // ───────────── FLAVOUR TEXT ─────────────
        if (dayFlavourText != null)
        {
            dayFlavourText.gameObject.SetActive(true);
            dayFlavourText.text = "";

            foreach (char c in flavourText)
            {
                dayFlavourText.text += c;
                yield return new WaitForSeconds(flavourTypeSpeed);
            }

            yield return new WaitForSeconds(flavourStayDuration);
            dayFlavourText.gameObject.SetActive(false);
        }

        // ───────────── ANIMATION ─────────────
        GameObject spawnedAnim = null;

        if (animationPrefab != null)
        {
            spawnedAnim = Instantiate(animationPrefab);

            Animator anim = spawnedAnim.GetComponent<Animator>();
            if (anim != null)
                anim.Play(0);
        }

        yield return new WaitForSeconds(animationDuration);

        if (spawnedAnim != null)
            Destroy(spawnedAnim);

        // ───────────── FINAL TEXT (NEXT CONTROLLED) ─────────────
        if (finalText != null && finalLines.Length > 0)
        {
            finalText.gameObject.SetActive(true);

            for (int i = 0; i < finalLines.Length; i++)
            {
                EndingLine line = finalLines[i];

                finalText.text = "";
                isTyping = true;
                skipRequested = false;
                nextPressed = false;

                foreach (char c in line.line)
                {
                    if (skipRequested) break;

                    finalText.text += c;
                    yield return new WaitForSeconds(line.typeSpeed);
                }

                // ensure full line is shown
                finalText.text = line.line;
                isTyping = false;

                // wait for NEXT press
                yield return new WaitUntil(() => nextPressed);
            }
        }
    }
}