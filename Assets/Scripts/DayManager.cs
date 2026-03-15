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
    public float dayStartScreenDuration = 3f;

    [Header("Patient Groups per Day")]
    public GameObject[] dayPatientGroups; // drag Day1Patients, Day2Patients etc in order

    [Header("Patient Tracking")]
    public List<GameObject> activePatients = new List<GameObject>();
    private int patientsAdmittedToday = 0;

    void Awake() => Instance = this;

    void Start()
    {
        // Disable all day groups first
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
        // Deteriorate all waiting patients
        foreach (GameObject p in activePatients)
            p.GetComponent<PatientHealth>()?.Deteriorate();

        // Clean up dead patients
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
        dayTitleText.text = "Day " + (currentDayIndex + 1);
        dayFlavourText.text = CurrentDay.flavourText;

        yield return new WaitForSeconds(dayStartScreenDuration);

        dayStartCanvas.SetActive(false);

        // Add daily resources and money
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

    // DEBUG
    public void DebugSkipDay() => EndDay();
    public void DebugForceAllDeteriorate()
    {
        foreach (GameObject p in activePatients)
            p.GetComponent<PatientHealth>()?.DebugForceDeteriorate();
    }
}