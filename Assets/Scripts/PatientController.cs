using UnityEngine;

public class PatientController : MonoBehaviour
{
    public PatientData data;
    [HideInInspector] public int denyCount = 0;
    void Start()
    {
        GetComponent<PatientHealth>()?.Init(data);
    }

    public void OnReachedCounter()
    {
        Debug.Log($"[COUNTER] {gameObject.name} reached counter. Data: {(data != null ? data.patientName : "NULL")}");

        if (data == null)
        {
            Debug.LogError($"[ERROR] {gameObject.name} has no PatientData assigned!");
            return;
        }

        PatientUIManager.Instance.ShowPatient(data, this); // pass self directly
    }

    public void ResumeWalking()
    {
        TopDownNPC npc = GetComponent<TopDownNPC>();
        npc.currentState = TopDownNPC.State.Walking;
        npc.ForcePickNewDirection();
        Debug.Log($"[WALK] {gameObject.name} resumed walking.");
    }
}