using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [Header("Special Ending")]
    public GameObject mayorFiredText;
    public static GameOverScreen Instance;
    public GameObject gameOverScreen;

    [Header("Game Over Reason Texts")]
    public GameObject familyDiedText;      // child of gameOverScreen
    public GameObject deathValueText;      // child of gameOverScreen

    [Header("Death Value Settings")]
    public int maxDeathValue = 50;
    [HideInInspector] public int currentDeathValue = 0;

    void Awake() => Instance = this;

    // Call when a patient dies — pass their death value
    public void AddDeathValue(int value)
    {
        currentDeathValue += value;
        Debug.Log($"[DEATH VALUE] Current: {currentDeathValue}/{maxDeathValue}");

        if (currentDeathValue >= maxDeathValue)
            TriggerGameOver(false);
    }
    void HideAllTexts()
    {
        if (familyDiedText != null)
            familyDiedText.SetActive(false);

        if (deathValueText != null)
            deathValueText.SetActive(false);

        if (mayorFiredText != null)
            mayorFiredText.SetActive(false);
    }
    // Call when a family member dies
    public void TriggerFamilyDeath()
    {
        TriggerGameOver(true);
    }

    void TriggerGameOver(bool familyDied)
    {
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);

        // Show correct text based on reason
        HideAllTexts();

        if (familyDied)
        {
            if (familyDiedText != null)
                familyDiedText.SetActive(true);
        }
        else
        {
            if (deathValueText != null)
                deathValueText.SetActive(true);
        }

        Debug.Log($"[GAME OVER] Reason: {(familyDied ? "Family died" : "Death value exceeded")}");
    }
    public void TriggerMayorFired()
    {
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);

        HideAllTexts();

        if (mayorFiredText != null)
            mayorFiredText.SetActive(true);

        Debug.Log("[GAME OVER] Reason: Mayor fired you");
    }

    public void onRestartPress()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}