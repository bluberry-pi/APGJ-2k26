using UnityEngine;
using System.Collections;

public class PatientHealth : MonoBehaviour
{
    public int currentHealth;
    private PatientData data;
    private SpriteRenderer spriteRenderer;
    public bool isDead = false;

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

        TopDownNPC npc = GetComponent<TopDownNPC>();
        if (npc != null)
            npc.currentState = TopDownNPC.State.Waiting;

        Debug.Log($"{data.patientName} has died.");

        // Only add death value if NOT Dad&Daughter
        if (!gameObject.CompareTag("Dad&Daughter"))
        {
            Debug.Log($"Death value added: {data.deathValue}");
            if (GameOverScreen.Instance != null)
                GameOverScreen.Instance.AddDeathValue(data.deathValue);
        }
        else
        {
            Debug.Log($"{data.patientName} died but death value skipped (Dad&Daughter tag).");
        }

        DayManager.Instance.RemovePatient(gameObject);

        // Always check for ending regardless of tag
        if (GameEnding.Instance != null)
            GameEnding.Instance.CheckForEnding();
    }

    public void DebugForceDeteriorate() => Deteriorate();
}