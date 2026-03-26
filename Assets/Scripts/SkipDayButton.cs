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
        if (button != null)
            button.interactable = false;
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

        if (disabledByAdmit || DayManager.Instance.isDayTransitioning) return;

        if (DayManager.Instance.currentDayIndex >= finalDayIndex)
        {
            Debug.Log("Final day reached → Loading next scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        DayManager.Instance.EndDay();
    }
}