using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipDayButton : MonoBehaviour
{
    public Button button;

    [Header("Settings")]
    public int finalDayIndex = 5;

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

        if (DayManager.Instance.currentDayIndex >= finalDayIndex)
        {
            Debug.Log("Final day reached → Loading next scene");

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }
        DayManager.Instance.EndDay();
    }
}