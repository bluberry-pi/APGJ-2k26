using UnityEngine;
public class SkipDayButton : MonoBehaviour
{
    public void SkipDay()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("SkipDayButton: DayManager.Instance is null.");
            return;
        }

        DayManager.Instance.EndDay();
    }
}