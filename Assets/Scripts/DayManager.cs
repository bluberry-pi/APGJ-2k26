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
    public TextMeshProUGUI moneyText;

    [Header("Flavour UI")]
    public TextMeshProUGUI dayFlavourText;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;
    public float fadeDuration = 0.8f;
    public float statsPauseDuration = 2f;
    public float statsFadeDuration = 0.5f;

    [Header("Patient Groups per Day")]
    public GameObject[] dayPatientGroups;

    [Header("Patient Tracking")]
    public List<GameObject> activePatients = new List<GameObject>();
    private int patientsAdmittedToday = 0;

    void Awake() => Instance = this;

    void Start()
    {
        foreach (GameObject group in dayPatientGroups)
            group.SetActive(false);

        fadeOverlay.color = new Color(0, 0, 0, 0);
        StartCoroutine(StartDay());
    }

    public DayData CurrentDay => allDays[currentDayIndex];

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
        yield return StartCoroutine(Fade(0f, 1f));

        dayStartCanvas.SetActive(true);
        statsGroup.SetActive(true);
        dayFlavourText.text = "";

        yield return StartCoroutine(StartDay());
    }

    IEnumerator StartDay()
    {
        if (currentDayIndex == 0)
        {
            yield return StartCoroutine(Fade(0f, 1f));
            dayStartCanvas.SetActive(true);
            statsGroup.SetActive(true);
            dayFlavourText.text = "";
        }

        // ── Phase 1: Stats ──────────────────────────────
        int currentRes = HospitalManager.Instance.currentRes;
        int currentMoney = HospitalManager.Instance.currentMoney;
        int resourceIncome = HospitalManager.Instance.ResPerDay;
        int rationCost = FamilyManager.Instance.dailyRationCost;

        dayTitleText.text = "";
        ResText.text = "";
        moneyText.text = "";

        yield return StartCoroutine(TypeText(dayTitleText, "Day " + (currentDayIndex + 1)));
        yield return new WaitForSeconds(0.3f);

        if (currentDayIndex == 0)
        {
            yield return StartCoroutine(TypeText(ResText, "Res: " + currentRes));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(TypeText(moneyText, "Money: $" + currentMoney));
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return StartCoroutine(TypeText(ResText, "Res: " + currentRes + "  (+" + resourceIncome + " delivered)"));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(TypeText(moneyText, "Money: $" + currentMoney + "  (-$" + rationCost + " rations)"));
            yield return new WaitForSeconds(0.2f);
        }

        // Pause so player can read
        yield return new WaitForSeconds(statsPauseDuration);

        // Fade stats out
        yield return StartCoroutine(FadeCanvasGroup(statsGroup, 1f, 0f, statsFadeDuration));
        statsGroup.SetActive(false);

        // ── Phase 2: Flavour text ───────────────────────
        dayFlavourText.text = "";
        foreach (DayFlavourLine flavour in CurrentDay.flavourLines)
        {
            dayFlavourText.text = "";
            yield return StartCoroutine(TypeText(dayFlavourText, flavour.line));
            yield return new WaitForSeconds(flavour.pauseAfter);
        }

        yield return StartCoroutine(FadeCanvasGroup(dayFlavourText.gameObject, 1f, 0f, statsFadeDuration));
        dayFlavourText.text = "";
        dayFlavourText.gameObject.GetComponent<CanvasGroup>().alpha = 1f; // reset for next day

        yield return new WaitForSeconds(0.5f);

        // Fade back in to reveal the scene
        yield return StartCoroutine(Fade(1f, 0f));

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Fade(1f, 0f));

        dayStartCanvas.SetActive(false);

        foreach (FamilySicknessEvent e in CurrentDay.familySicknessOverrides)
            FamilyManager.Instance.ForceSetSick(e.memberName, e.forceSick);

        HospitalManager.Instance.AddDailyIncome();

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