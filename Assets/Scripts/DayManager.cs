using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Day Setup")]
    public DayData[] allDays;
    public int currentDayIndex = 0;

    [Header("Day Start UI")]
    public GameObject dayStartCanvas;
    public TextMeshProUGUI dayTitleText;
    public TextMeshProUGUI dayFlavourText;
    public TextMeshProUGUI timerText; // shows countdown

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

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
        // iterate over a copy so removals don't break the loop
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

        StartCoroutine(StartDay());
    }

    IEnumerator StartDay()
    {
        dayStartCanvas.SetActive(true);

        dayTitleText.text = "";
        yield return StartCoroutine(TypeText(dayTitleText, "Day " + (currentDayIndex + 1)));

        yield return new WaitForSeconds(0.5f);

        foreach (DayFlavourLine flavour in CurrentDay.flavourLines)
        {
            dayFlavourText.text = "";
            yield return StartCoroutine(TypeText(dayFlavourText, flavour.line));
            yield return new WaitForSeconds(flavour.pauseAfter); // invisible pause
        }

        yield return new WaitForSeconds(0.5f);
        dayStartCanvas.SetActive(false);
        foreach (FamilySicknessEvent e in CurrentDay.familySicknessOverrides)
        {
            FamilyManager.Instance.ForceSetSick(e.memberName, e.forceSick);
        }

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

    IEnumerator CountDown(float duration)
    {
        float remaining = duration;

        while (remaining > 0f)
        {
            if (timerText != null)
                timerText.text = Mathf.Ceil(remaining).ToString();

            remaining -= Time.deltaTime;
            yield return null;
        }

        if (timerText != null)
            timerText.text = "";
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

    // DEBUG
    public void DebugSkipDay() => EndDay();
    public void DebugForceAllDeteriorate()
    {
        foreach (GameObject p in activePatients)
            p.GetComponent<PatientHealth>()?.DebugForceDeteriorate();
    }
}