using UnityEngine;

public class PatientController : MonoBehaviour
{
    public PatientData data;

    // 🔹 Dialogue (per visit)
    [HideInInspector] public int denyCountThisVisit = 0;

    // 🔹 Memory (persistent)
    [HideInInspector] public int totalDenies = 0;
    [HideInInspector] public int lastDeniedDay = -1;

    void Start()
    {
        GetComponent<PatientHealth>()?.Init(data);
    }

    public void OnReachedCounter()
    {
        // ✅ Reset ONLY per visit
        denyCountThisVisit = 0;

        Debug.Log($"[COUNTER] {gameObject.name} reached counter.");

        if (data == null)
        {
            Debug.LogError($"[ERROR] {gameObject.name} has no PatientData!");
            return;
        }

        PatientUIManager.Instance.ShowPatient(data, this);
    }

    public void ResumeWalking()
    {
        TopDownNPC npc = GetComponent<TopDownNPC>();
        npc.currentState = TopDownNPC.State.Walking;
        npc.ForcePickNewDirection();

        Debug.Log($"[WALK] {gameObject.name} resumed walking.");
    }
}