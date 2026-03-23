using UnityEngine;

[System.Serializable]
public class DayFlavourLine
{
    [TextArea(2, 4)]
    public string line;
    public float pauseAfter = 1.5f;
}

[CreateAssetMenu(fileName = "NewDay", menuName = "Hospital/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public int maxPatientsToAdmit;
    public DayFlavourLine[] flavourLines;
}