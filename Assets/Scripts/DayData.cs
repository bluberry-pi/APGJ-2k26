using UnityEngine;

[System.Serializable]
public class DayFlavourLine
{
    [TextArea(2, 4)]
    public string line;
    public float pauseAfter = 1.5f;
}

[System.Serializable]
public class FamilySicknessEvent
{
    public string memberName; // "Wife", "Son", "Daughter"
    public bool forceSick;    // true = force sick, false = force healthy
}

[CreateAssetMenu(fileName = "NewDay", menuName = "Hospital/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public int maxPatientsToAdmit;
    public DayFlavourLine[] flavourLines;

    [Header("Manual Family Sickness — leave empty for random only")]
    public FamilySicknessEvent[] familySicknessOverrides;
}