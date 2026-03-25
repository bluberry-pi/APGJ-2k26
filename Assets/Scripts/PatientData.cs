using UnityEngine;

public enum PatientTier { Tier1, Tier2, Tier3 }

[CreateAssetMenu(fileName = "NewPatient", menuName = "Hospital/PatientData")]
public class PatientData : ScriptableObject
{
    [Header("Basic Info")]
    public string patientName;
    public int age;
    public string condition;
    [TextArea(3, 6)]
    public string bio;

    [Header("Tier & Reward")]
    public PatientTier tier;
    public int rewardMoney;

    [Header("Health")]
    public int maxHealth = 100;
    public int deteriorationPerDay = 20;

    [Header("Sprites")]
    public Sprite deadSprite;

    [Header("Resources")]
    public int resourceCost;

    [Header("Death Value")]
    public int deathValue = 10; // added to total when this patient dies

    [Header("Dialogue")]
    public DialogueData dialogue;
}