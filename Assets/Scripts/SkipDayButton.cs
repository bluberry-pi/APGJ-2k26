using UnityEngine;
using UnityEngine.UI;

public class SkipDayButton : MonoBehaviour
{
    public Button button;

    void Update()
    {
        if (button != null && DayManager.Instance != null)
            button.interactable = !DayManager.Instance.isDayTransitioning;
    }

    public void SkipDay()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("SkipDayButton: DayManager.Instance is null.");
            return;
        }

        if (DayManager.Instance.isDayTransitioning) return;

        DayManager.Instance.EndDay();
    }
}