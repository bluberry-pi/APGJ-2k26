using UnityEngine;
using System.Collections.Generic;

public class PatientTracker : MonoBehaviour
{
    public static PatientTracker Instance;

    [Header("Drag every patient from hierarchy here")]
    public GameObject[] allPatients;

    [Header("Settings")]
    public int clearThreshold = 3; // when active patients drops to this or below, clear next day end

    [HideInInspector] public bool shouldClearNextDay = false;

    void Awake() => Instance = this;

    // Returns how many patients are still alive in the array
    public int GetActivePatientCount()
    {
        int count = 0;
        foreach (GameObject p in allPatients)
        {
            if (p != null)
            {
                PatientHealth ph = p.GetComponent<PatientHealth>();
                if (ph != null && !ph.isDead)
                    count++;
            }
        }
        return count;
    }

    // Called by DayManager at end of every day
    public void CheckAndFlagClear()
    {
        int active = GetActivePatientCount();
        Debug.Log($"Active patients remaining: {active}");

        if (active <= clearThreshold)
        {
            shouldClearNextDay = true;
            Debug.Log("Patient count low — all non-tagged patients will be cleared next day.");
        }
    }

    // Called by DayManager at start of next day if flagged
    public void ClearNonTaggedPatients()
    {
        foreach (GameObject p in allPatients)
        {
            if (p == null) continue;
            if (p.CompareTag("Dad&Daughter")) continue;

            DayManager.Instance.RemovePatient(p);
            Destroy(p);
        }

        shouldClearNextDay = false;
        Debug.Log("All non-tagged patients cleared.");
    }
}