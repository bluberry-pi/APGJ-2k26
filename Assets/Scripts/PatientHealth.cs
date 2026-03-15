using UnityEngine;

public class PatientHealth : MonoBehaviour
{
    public int currentHealth;
    private PatientData data;

    public void Init(PatientData patientData)
    {
        data = patientData;
        currentHealth = patientData.maxHealth;
    }

    // Called by DayManager at end of each day
    public void Deteriorate()
    {
        currentHealth -= data.deteriorationPerDay;
        Debug.Log($"{data.patientName} health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{data.patientName} has died.");
            DayManager.Instance.RemovePatient(gameObject);
            Destroy(gameObject);
        }
    }

    // DEBUG — call from inspector or debug panel
    public void DebugForceDeteriorate()
    {
        Deteriorate();
    }
}