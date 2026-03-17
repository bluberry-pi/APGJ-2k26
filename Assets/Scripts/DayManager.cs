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

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;
    public float linePauseTime = 1.5f; // pause between lines automatically

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
        foreach (GameObject p in activePatients)
            p.GetComponent<PatientHealth>()?.Deteriorate();

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

        // Type day title
        dayTitleText.text = "";
        string title = "Day " + (currentDayIndex + 1);
        yield return StartCoroutine(TypeText(dayTitleText, title));

        yield return new WaitForSeconds(0.5f);

        // Type each flavour line one by one with pause between
        foreach (string line in CurrentDay.flavourLines)
        {
            dayFlavourText.text = "";
            yield return StartCoroutine(TypeText(dayFlavourText, line));
            yield return new WaitForSeconds(linePauseTime);
        }

        // Auto close after last line
        yield return new WaitForSeconds(1f);
        dayStartCanvas.SetActive(false);

        HospitalManager.Instance.AddDailyIncome();

        // Enable this day's patients
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