using UnityEngine;
using System.Collections;

public class PatientHealth : MonoBehaviour
{
    public int currentHealth;
    private PatientData data;
    private SpriteRenderer spriteRenderer;
    public bool isDead = false; // changed to public so TopDownNPC can check it

    public void Init(PatientData patientData)
    {
        data = patientData;
        currentHealth = patientData.maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Deteriorate()
    {
        if (isDead) return;
        currentHealth -= data.deteriorationPerDay;
        Debug.Log($"{data.patientName} health: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;

        if (spriteRenderer != null && data.deadSprite != null)
            spriteRenderer.sprite = data.deadSprite;

        // Set state to Waiting so they stop moving
        // but also flag as dead so clicks are blocked
        TopDownNPC npc = GetComponent<TopDownNPC>();
        if (npc != null)
            npc.currentState = TopDownNPC.State.Waiting;

        Debug.Log($"{data.patientName} has died.");
        DayManager.Instance.RemovePatient(gameObject);
    }

    public void DebugForceDeteriorate() => Deteriorate();
}