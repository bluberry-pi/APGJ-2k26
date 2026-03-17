using UnityEngine;

[CreateAssetMenu(fileName = "NewDay", menuName = "Hospital/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public int maxPatientsToAdmit;
    [TextArea(2, 4)]
    public string[] flavourLines; // multiple lines, one per pause
}