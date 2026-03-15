using UnityEngine;

public class PatientController : MonoBehaviour
{
    public PatientData data;

    void Start()
    {
        GetComponent<PatientHealth>()?.Init(data);
        // No self registration needed, DayManager handles it
    }

    public void OnReachedCounter()
    {
        PatientUIManager.Instance.ShowPatient(data);
    }

    public void ResumeWalking()
    {
        TopDownNPC npc = GetComponent<TopDownNPC>();
        npc.currentState = TopDownNPC.State.Walking;
        npc.ForcePickNewDirection();
    }
}