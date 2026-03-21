using UnityEngine;
using System.Collections;

public class PatientHealth : MonoBehaviour
{
    public int currentHealth;
    private PatientData data;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    public void Init(PatientData patientData)
    {
        data = patientData;
        currentHealth = patientData.maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        // no sprite set here — whatever is on the prefab stays
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

        // Swap to dead sprite
        if (spriteRenderer != null && data.deadSprite != null)
            spriteRenderer.sprite = data.deadSprite;

        // Stop moving
        TopDownNPC npc = GetComponent<TopDownNPC>();
        if (npc != null)
            npc.currentState = TopDownNPC.State.Waiting;

        Debug.Log($"{data.patientName} has died.");

        DayManager.Instance.RemovePatient(gameObject);
    }

    // DEBUG
    public void DebugForceDeteriorate() => Deteriorate();
}