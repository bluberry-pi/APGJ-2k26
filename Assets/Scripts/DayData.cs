using UnityEngine;

[System.Serializable]
public class DayFlavourLine
{
    [TextArea(2, 4)]
    public string line;
    public float pauseAfter = 1.5f;
}

[System.Serializable]
public class DayEconomy
{
    [Header("Resources")]
    public int resourcesAdded = 20;
    public int resourcesDeducted = 0;

    [Header("Money")]
    public int moneyAdded = 0;
    public int moneyDeducted = 20;
}

[CreateAssetMenu(fileName = "NewDay", menuName = "Hospital/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public int maxPatientsToAdmit;
    public DayFlavourLine[] flavourLines;
    public DayEconomy economy;
}