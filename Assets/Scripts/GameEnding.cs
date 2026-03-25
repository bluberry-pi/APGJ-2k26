using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    public static GameEnding Instance;

    [Header("Ending Settings")]
    public string endingSceneName = "EndingScene"; // set in inspector
    public float delayBeforeEnd = 1.5f;

    private bool endingTriggered = false;

    void Awake()
    {
        Instance = this;
    }

    public void CheckForEnding()
    {
        if (endingTriggered) return;

        if (PatientTracker.Instance == null) return;

        int alivePatients = PatientTracker.Instance.GetActivePatientCount();

        Debug.Log("[ENDING CHECK] Alive patients: " + alivePatients);

        if (alivePatients <= 0)
        {
            Debug.Log("[ENDING] All patients dead — triggering ending.");
            endingTriggered = true;
            StartCoroutine(TriggerEnding());
        }
    }

    System.Collections.IEnumerator TriggerEnding()
    {
        yield return new WaitForSeconds(delayBeforeEnd);

        Time.timeScale = 1f; // reset just in case
        SceneManager.LoadScene(endingSceneName);
    }
}