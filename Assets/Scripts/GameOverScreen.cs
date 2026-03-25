using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
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
        if (familyDiedText != null)
            familyDiedText.SetActive(familyDied);

        if (deathValueText != null)
            deathValueText.SetActive(!familyDied);

        Debug.Log($"[GAME OVER] Reason: {(familyDied ? "Family died" : "Death value exceeded")}");
    }

    public void onRestartPress()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}