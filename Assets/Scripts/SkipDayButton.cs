using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipDayButton : MonoBehaviour
{
    public Button button;

    [Header("Settings")]
    public int finalDayIndex = 5;

    private bool disabledByAdmit = false;

    void Update()
    {
        if (button == null || DayManager.Instance == null) return;

        if (disabledByAdmit || DayManager.Instance.isDayTransitioning)
            button.interactable = false;
        else
            button.interactable = true;
    }

    public void DisableImmediately()
    {
        disabledByAdmit = true;
        Debug.Log("[SKIPBTN] DisableImmediately called — disabledByAdmit=true");

        if (button == null)
            Debug.LogError("[SKIPBTN] button is NULL — not assigned in Inspector!");
        else
        {
            button.interactable = false;
            Debug.Log($"[SKIPBTN] button.interactable set to false. Current value: {button.interactable}");
        }
    }

    public void ResetForNewDay()
    {
        disabledByAdmit = false;
    }

    public void SkipDay()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("SkipDayButton: DayManager.Instance is null.");
            return;
        }

        Debug.Log($"[SKIPBTN] SkipDay called — disabledByAdmit={disabledByAdmit}, isDayTransitioning={DayManager.Instance.isDayTransitioning}, dayIndex={DayManager.Instance.currentDayIndex}");

        if (disabledByAdmit || DayManager.Instance.isDayTransitioning)
        {
            Debug.Log("[SKIPBTN] SkipDay BLOCKED.");
            return;
        }

        // currentDayIndex is the day we're ON (0-based)
        // finalDayIndex is the last valid day index (e.g. 5 for a 6-day game)
        // After EndDay() on final day, currentDayIndex becomes finalDayIndex+1
        // So check >= finalDayIndex to catch both: pressing skip on last day, or after it ends
        if (DayManager.Instance.currentDayIndex >= finalDayIndex)
        {
            Debug.Log("Final day reached → Loading next scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        DayManager.Instance.EndDay();
    }
}