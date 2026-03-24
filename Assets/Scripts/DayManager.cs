using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Day Setup")]
    public DayData[] allDays;
    public int currentDayIndex = 0;

    [Header("Fade")]
    public Image fadeOverlay;

    [Header("Day Start UI")]
    public GameObject dayStartCanvas;
    public TextMeshProUGUI dayTitleText;

    [Header("Stats UI")]
    public GameObject statsGroup;
    public TextMeshProUGUI ResText;
    public TextMeshProUGUI resDeltaText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI moneyDeltaText;

    [Header("Flavour UI")]
    public TextMeshProUGUI dayFlavourText;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;
    public float fadeDuration = 0.8f;
    public float statsPauseDuration = 2f;
    public float statsFadeDuration = 0.5f;
    public float deltaFadeDuration = 0.5f;

    [Header("Patient Groups per Day")]
    public GameObject[] dayPatientGroups;

    [Header("Patient Tracking")]
    public List<GameObject> activePatients = new List<GameObject>();
    private int patientsAdmittedToday = 0;

    [Header("Purge Day Settings")]
    [Tooltip("Day NUMBER (1-based) on which all patients except 'Dad&Daughter' are destroyed. Set to 0 to disable.")]
    public int purgeDayNumber = 5;

    [Header("Debug")]
    public bool skipDayCanvas = false;

    // True while the purge day end is being processed.
    // PatientHealth.Die() checks this to suppress game-over for patients
    // that are about to be force-destroyed anyway.
    [HideInInspector] public bool isPurgeDay = false;

    void Awake() => Instance = this;

    void Start()
    {
        foreach (GameObject group in dayPatientGroups)
            group.SetActive(false);

        fadeOverlay.color = new Color(0, 0, 0, 0);
        StartCoroutine(StartDay());
    }

    public DayData CurrentDay => allDays[currentDayIndex];

    // currentDayIndex is 0-based; purgeDayNumber is 1-based (matches "Day 5" in the Inspector).
    bool IsCurrentDayPurgeDay => purgeDayNumber > 0 && (currentDayIndex + 1) == purgeDayNumber;

    public bool CanAdmitMore()
    {
        return patientsAdmittedToday < CurrentDay.maxPatientsToAdmit;
    }

    public void PatientAdmitted()
    {
        patientsAdmittedToday++;
        if (patientsAdmittedToday >= CurrentDay.maxPatientsToAdmit)
            StartCoroutine(DelayedEndDay());
    }

    public void RemovePatient(GameObject patient)
    {
        if (activePatients.Contains(patient))
            activePatients.Remove(patient);
    }

    IEnumerator DelayedEndDay()
    {
        yield return new WaitForSeconds(2f);
        EndDay();
    }

    public void EndDay()
    {
        if (IsCurrentDayPurgeDay)
        {
            // ── PURGE DAY ──────────────────────────────────────────────────────
            // Raise the flag so PatientHealth.Die() knows not to trigger game-over
            // for patients that are being silently wiped this day.
            isPurgeDay = true;

            List<GameObject> patientsCopy = new List<GameObject>(activePatients);
            foreach (GameObject p in patientsCopy)
            {
                if (p == null) continue;

                // Spare any patient tagged "Dad&Daughter" — they continue to next day.
                if (p.CompareTag("Dad&Daughter")) continue;

                // Remove from the tracking list first so no stale references remain.
                activePatients.Remove(p);
                Destroy(p);
            }

            // Clean up any remaining nulls (e.g. Dad&Daughter if they somehow died).
            activePatients.RemoveAll(p => p == null);

            isPurgeDay = false;
            // ──────────────────────────────────────────────────────────────────
        }
        else
        {
            // Normal day-end: apply deterioration to every remaining patient.
            List<GameObject> patientsCopy = new List<GameObject>(activePatients);
            foreach (GameObject p in patientsCopy)
            {
                if (p != null)
                    p.GetComponent<PatientHealth>()?.Deteriorate();
            }

            activePatients.RemoveAll(p => p == null);
        }

        FamilyManager.Instance.EndOfDayUpdate();

        patientsAdmittedToday = 0;
        currentDayIndex++;

        if (currentDayIndex >= allDays.Length)
        {
            Debug.Log("Game over");
            return;
        }

        StartCoroutine(TransitionToNextDay());
    }

    IEnumerator TransitionToNextDay()
    {
        if (!skipDayCanvas)
        {
            FamilyManager.Instance.paused = true;
            yield return StartCoroutine(Fade(0f, 1f));
            dayStartCanvas.SetActive(true);
            statsGroup.SetActive(true);
            dayFlavourText.text = "";
        }

        yield return StartCoroutine(StartDay());
    }

    IEnumerator StartDay()
    {
        if (!skipDayCanvas)
        {
            if (currentDayIndex == 0)
            {
                FamilyManager.Instance.paused = true;
                yield return StartCoroutine(Fade(0f, 1f));
                dayStartCanvas.SetActive(true);
                statsGroup.SetActive(true);
                dayFlavourText.text = "";
            }

            // Apply economy for this day
            HospitalManager.Instance.ApplyDayEconomy(CurrentDay.economy);

            int currentRes = HospitalManager.Instance.currentResources;
            int currentMoney = HospitalManager.Instance.currentMoney;
            DayEconomy economy = CurrentDay.economy;

            // Clear all texts
            dayTitleText.text = "";
            ResText.text = "";
            resDeltaText.text = "";
            moneyText.text = "";
            moneyDeltaText.text = "";

            // Make deltas invisible and hide on day 1
            resDeltaText.gameObject.SetActive(currentDayIndex > 0);
            moneyDeltaText.gameObject.SetActive(currentDayIndex > 0);

            if (currentDayIndex > 0)
            {
                Color invisible = resDeltaText.color;
                invisible.a = 0f;
                resDeltaText.color = invisible;
                moneyDeltaText.color = invisible;
            }

            // Type day title
            yield return StartCoroutine(TypeText(dayTitleText, "Day " + (currentDayIndex + 1)));
            yield return new WaitForSeconds(0.3f);

            // Type resources then fade in delta
            yield return StartCoroutine(TypeText(ResText, "Res: " + currentRes));
            if (currentDayIndex > 0)
            {
                resDeltaText.text = BuildDelta(economy.resourcesAdded, economy.resourcesDeducted, "res");
                yield return StartCoroutine(FadeInText(resDeltaText, deltaFadeDuration));
            }
            yield return new WaitForSeconds(0.2f);

            // Type money then fade in delta
            yield return StartCoroutine(TypeText(moneyText, "$ " + currentMoney));
            if (currentDayIndex > 0)
            {
                moneyDeltaText.text = BuildDelta(economy.moneyAdded, economy.moneyDeducted, "rations");
                yield return StartCoroutine(FadeInText(moneyDeltaText, deltaFadeDuration));
            }
            yield return new WaitForSeconds(0.2f);

            // Pause so player can read
            yield return new WaitForSeconds(statsPauseDuration);

            // Fade stats out
            yield return StartCoroutine(FadeCanvasGroup(statsGroup, 1f, 0f, statsFadeDuration));
            statsGroup.SetActive(false);
            CanvasGroup statsCG = statsGroup.GetComponent<CanvasGroup>();
            if (statsCG != null) statsCG.alpha = 1f;

            // Flavour text
            dayFlavourText.text = "";
            foreach (DayFlavourLine flavour in CurrentDay.flavourLines)
            {
                dayFlavourText.text = "";
                yield return StartCoroutine(TypeText(dayFlavourText, flavour.line));
                yield return new WaitForSeconds(flavour.pauseAfter);
            }

            // Fade flavour out
            yield return StartCoroutine(FadeCanvasGroup(dayFlavourText.gameObject, 1f, 0f, statsFadeDuration));
            dayFlavourText.text = "";
            CanvasGroup flavourCG = dayFlavourText.gameObject.GetComponent<CanvasGroup>();
            if (flavourCG != null) flavourCG.alpha = 1f;

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Fade(1f, 0f));
            dayStartCanvas.SetActive(false);
        }
        else
        {
            // Skip canvas but still apply economy
            HospitalManager.Instance.ApplyDayEconomy(CurrentDay.economy);
        }

        // Always runs
        if (currentDayIndex < dayPatientGroups.Length)
        {
            GameObject group = dayPatientGroups[currentDayIndex];
            group.SetActive(true);

            foreach (Transform child in group.transform)
            {
                PatientController pc = child.GetComponent<PatientController>();
                if (pc != null && !activePatients.Contains(child.gameObject))
                    activePatients.Add(child.gameObject);
            }
        }

        StartCoroutine(ResumeHealthDrain());
    }

    IEnumerator ResumeHealthDrain()
    {
        yield return new WaitForSeconds(skipDayCanvas ? 0f : 2f);
        FamilyManager.Instance.paused = false;
    }

    IEnumerator FadeInText(TextMeshProUGUI textField, float duration)
    {
        Color c = textField.color;
        c.a = 0f;
        textField.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            textField.color = c;
            yield return null;
        }

        c.a = 1f;
        textField.color = c;
    }

    string BuildDelta(int added, int deducted, string label)
    {
        string result = "";
        if (added > 0)    result += "+" + added + " ";
        if (deducted > 0) result += "-" + deducted + " " + label;
        return result.Trim();
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        fadeOverlay.color = new Color(0, 0, 0, from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, to);
    }

    IEnumerator FadeCanvasGroup(GameObject obj, float from, float to, float duration)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    IEnumerator TypeText(TextMeshProUGUI textField, string line)
    {
        textField.text = "";
        foreach (char c in line)
        {
            textField.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    public void DebugSkipDay() => EndDay();
    public void DebugForceAllDeteriorate()
    {
        foreach (GameObject p in activePatients)
            p.GetComponent<PatientHealth>()?.DebugForceDeteriorate();
    }
}