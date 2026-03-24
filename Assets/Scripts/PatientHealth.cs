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

        // If this death is happening during the purge day, skip all death consequences
        // (sprite swap, game-over, etc.) for patients that are about to be destroyed anyway.
        // Dad&Daughter are never destroyed on purge day, so their deaths still count normally.
        if (DayManager.Instance != null &&
            DayManager.Instance.isPurgeDay &&
            !gameObject.CompareTag("Dad&Daughter"))
        {
            Debug.Log($"{data.patientName} wiped on purge day — death suppressed.");
            DayManager.Instance.RemovePatient(gameObject);
            return;
        }

        // Normal death flow
        if (spriteRenderer != null && data.deadSprite != null)
            spriteRenderer.sprite = data.deadSprite;

        TopDownNPC npc = GetComponent<TopDownNPC>();
        if (npc != null)
            npc.currentState = TopDownNPC.State.Waiting;

        Debug.Log($"{data.patientName} has died.");

        DayManager.Instance.RemovePatient(gameObject);
    }

    // DEBUG
    public void DebugForceDeteriorate() => Deteriorate();
}