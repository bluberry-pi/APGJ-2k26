using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;
    [Header("Intro UI")]
    public GameObject introUIToDestroy;

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
    public GameObject flavourNextButton;

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

    [Header("Debug")]
    public bool skipDayCanvas = false;

    // ✅ NEW FLAG
    private bool flavourNextPressed = false;

    void Awake() => Instance = this;

    void Start()
    {
        foreach (GameObject group in dayPatientGroups)
            group.SetActive(false);

        fadeOverlay.color = new Color(0, 0, 0, 0);
        StartCoroutine(StartDay());
    }
    public DayData CurrentDay => allDays[currentDayIndex];

    public void OnFlavourNextPressed()
    {
        flavourNextPressed = true;
    }

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
        List<GameObject> patientsCopy = new List<GameObject>(activePatients);
        foreach (GameObject p in patientsCopy)
        {
            if (p != null)
                p.GetComponent<PatientHealth>()?.Deteriorate();
        }

        activePatients.RemoveAll(p => p == null);

        PatientTracker.Instance.CheckAndFlagClear();
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
        MusicManager.Instance.PlayDayAmbient(currentDayIndex);
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

            HospitalManager.Instance.ApplyDayEconomy(CurrentDay.economy);

            int currentRes = HospitalManager.Instance.currentResources;
            int currentMoney = HospitalManager.Instance.currentMoney;
            DayEconomy economy = CurrentDay.economy;

            dayTitleText.text = "";
            ResText.text = "";
            resDeltaText.text = "";
            moneyText.text = "";
            moneyDeltaText.text = "";

            resDeltaText.gameObject.SetActive(currentDayIndex > 0);
            moneyDeltaText.gameObject.SetActive(currentDayIndex > 0);

            if (currentDayIndex > 0)
            {
                Color invisible = resDeltaText.color;
                invisible.a = 0f;
                resDeltaText.color = invisible;
                moneyDeltaText.color = invisible;
            }

            yield return StartCoroutine(TypeText(dayTitleText, "Day " + (currentDayIndex + 1)));
            yield return new WaitForSeconds(0.3f);

            yield return StartCoroutine(TypeText(ResText, "Res: " + currentRes));
            if (currentDayIndex > 0)
            {
                resDeltaText.text = BuildDelta(economy.resourcesAdded, economy.resourcesDeducted, "res");
                yield return StartCoroutine(FadeInText(resDeltaText, deltaFadeDuration));
            }
            yield return new WaitForSeconds(0.2f);

            yield return StartCoroutine(TypeText(moneyText, "$ " + currentMoney));
            if (currentDayIndex > 0)
            {
                moneyDeltaText.text = BuildDelta(economy.moneyAdded, economy.moneyDeducted, "rations");
                yield return StartCoroutine(FadeInText(moneyDeltaText, deltaFadeDuration));
            }
            yield return new WaitForSeconds(0.2f);

            yield return new WaitForSeconds(statsPauseDuration);

            yield return StartCoroutine(FadeCanvasGroup(statsGroup, 1f, 0f, statsFadeDuration));
            statsGroup.SetActive(false);

            CanvasGroup statsCG = statsGroup.GetComponent<CanvasGroup>();
            if (statsCG != null) statsCG.alpha = 1f;

            // ✅ NEW FLAVOUR SYSTEM
            dayFlavourText.text = "";
            flavourNextButton.SetActive(true);

            foreach (DayFlavourLine flavour in CurrentDay.flavourLines)
            {
                flavourNextPressed = false;
                dayFlavourText.text = "";

                foreach (char c in flavour.line)
                {
                    if (flavourNextPressed) break;

                    dayFlavourText.text += c;
                    yield return new WaitForSeconds(typewriterSpeed);
                }

                if (flavourNextPressed)
                {
                    dayFlavourText.text = flavour.line;
                    flavourNextPressed = false;

                    yield return new WaitUntil(() => flavourNextPressed);
                }
                else
                {
                    yield return new WaitUntil(() => flavourNextPressed);
                }
            }

            flavourNextButton.SetActive(false);

            yield return StartCoroutine(FadeCanvasGroup(dayFlavourText.gameObject, 1f, 0f, statsFadeDuration));
            dayFlavourText.text = "";

            CanvasGroup flavourCG = dayFlavourText.gameObject.GetComponent<CanvasGroup>();
            if (flavourCG != null) flavourCG.alpha = 1f;

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Fade(1f, 0f));
            dayStartCanvas.SetActive(false);
            yield return StartCoroutine(Fade(1f, 0f));
            dayStartCanvas.SetActive(false);
            if (introUIToDestroy != null)
            {
                Destroy(introUIToDestroy);
                introUIToDestroy = null;
            }
        }
        else
        {
            HospitalManager.Instance.ApplyDayEconomy(CurrentDay.economy);
        }

        if (PatientTracker.Instance.shouldClearNextDay)
            PatientTracker.Instance.ClearNonTaggedPatients();

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
        if (added > 0) result += "+" + added + " ";
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