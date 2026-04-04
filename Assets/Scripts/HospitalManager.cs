using System.Collections;
using UnityEngine;
using TMPro;

public class HospitalManager : MonoBehaviour
{
    public static HospitalManager Instance;

    [Header("Starting Values")]
    public int startingMoney = 200;
    public int startingResources = 100;

    [HideInInspector] public int currentMoney;
    [HideInInspector] public int currentResources;

    [Header("Displays")]
    public TextMeshProUGUI moneyDisplay;
    public TextMeshProUGUI resourceDisplay;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentMoney = startingMoney;
        currentResources = startingResources;
    }

    void Start() => UpdateDisplay();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddDebugResources(10);
        }
    }
    void AddDebugResources(int amount)
    {
        currentResources += amount;

        Debug.Log($"[DEBUG] Added {amount} resources. Total: {currentResources}");

        UpdateDisplay();
    }
    public bool CanAfford(int cost) => currentResources >= cost;
    public bool CanAffordMoney(int cost) => currentMoney >= cost;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateDisplay();
    }

    public void SpendMoney(int amount)
    {
        currentMoney -= amount;
        UpdateDisplay();
    }

    public void AddResources(int amount)
    {
        currentResources += amount;
        UpdateDisplay();
    }

    public void SpendResources(int amount)
    {
        currentResources -= amount;
        UpdateDisplay();
    }

    // Called by DayManager at start of each new day
    public void ApplyDayEconomy(DayEconomy economy)
    {
        currentResources += economy.resourcesAdded;
        currentResources -= economy.resourcesDeducted;
        currentMoney += economy.moneyAdded;
        currentMoney -= economy.moneyDeducted;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (moneyDisplay != null)
            moneyDisplay.text = "$ " + currentMoney;
        if (resourceDisplay != null)
            resourceDisplay.text = "Res: " + currentResources;
    }

    // =============================================
    // ADMIT BUTTON
    // =============================================
    private bool isAdmitting = false;

    public void OnAdmitPressed()
    {
        if (isAdmitting)
        {
            Debug.Log("[ADMIT] Already processing an admit — ignored.");
            return;
        }

        if (!DayManager.Instance.CanAdmitMore())
        {
            Debug.Log("Max patients admitted today.");
            return;
        }

        PatientData p = PatientUIManager.Instance.currentPatient;
        if (p == null) return;

        if (!CanAfford(p.resourceCost))
        {
            Debug.Log("Not enough resources.");
            return;
        }

        isAdmitting = true;
        PatientUIManager.Instance.SetButtonsInteractable(false);

        string dayAdmitID = "onAdmit_Day" + (DayManager.Instance.currentDayIndex + 1);
        DialogueSequence seq = p.dialogue?.GetSequence(dayAdmitID);

        string sequenceToPlay = seq != null ? dayAdmitID : "onAdmit";

        DialogueManager.Instance.PlaySequence(
            p.dialogue,
            sequenceToPlay,
            onComplete: () => CompleteAdmit(p)
        );
    }

    void CompleteAdmit(PatientData p)
    {
        isAdmitting = false;
        currentResources -= p.resourceCost;
        AddMoney(p.rewardMoney);

        GameObject patientObj = PatientUIManager.Instance.currentPatientController?.gameObject;

        PatientUIManager.Instance.HideInterface();

        if (patientObj != null)
        {
            DayManager.Instance.RemovePatient(patientObj);
            Destroy(patientObj);
        }

        DayManager.Instance.PatientAdmitted();

        Debug.Log($"Admitted {p.patientName}. Earned ${p.rewardMoney}.");
    }

    // =============================================
    // DENY BUTTON
    // =============================================

    public void OnDenyPressed()
    {
        PatientData p = PatientUIManager.Instance.currentPatient;
        PatientController controller = PatientUIManager.Instance.currentPatientController;

        if (p == null || controller == null) return;

        int currentDay = DayManager.Instance.currentDayIndex;

        if (controller.lastDeniedDay != currentDay)
        {
            controller.totalDenies++;
            controller.lastDeniedDay = currentDay;
        }

        string denyID = $"onDeny_Day{controller.totalDenies}_{controller.denyCountThisVisit + 1}";
        Debug.Log("Trying sequence: " + denyID);

        DialogueSequence seq = p.dialogue?.GetSequence(denyID);

        if (seq == null)
        {
            // No dialogue at all for this deny — just leave immediately
            PatientUIManager.Instance.currentPatientController?.ResumeWalking();
            PatientUIManager.Instance.HideInterface();
            DialogueManager.Instance.ForceClose();
            return;
        }

        controller.denyCountThisVisit++;

        bool isPolitician = controller.CompareTag("Politician");
        bool triggerMayorEnding = isPolitician && controller.totalDenies >= 2;

        PlayDenySequenceChain(p, controller, seq.sequenceID, triggerMayorEnding);
    }

    void PlayDenySequenceChain(PatientData p, PatientController controller, string sequenceID, bool triggerMayorEnding)
    {
        DialogueManager.Instance.PlaySequence(
            p.dialogue,
            sequenceID,
            onComplete: () =>
            {
                if (triggerMayorEnding)
                {
                    GameOverScreen.Instance?.TriggerMayorFired();
                    return;
                }

                // Check if there's a next line in the chain
                string nextID = $"onDeny_Day{controller.totalDenies}_{controller.denyCountThisVisit + 1}";
                Debug.Log($"[DENY CHAIN] Checking next: {nextID}");
                DialogueSequence nextSeq = p.dialogue?.GetSequence(nextID);

                if (nextSeq != null)
                {
                    // More dialogue — advance counter and keep playing
                    controller.denyCountThisVisit++;
                    PlayDenySequenceChain(p, controller, nextSeq.sequenceID, triggerMayorEnding);
                    return;
                }

                // No more dialogue — clean up and let patient leave
                Debug.Log("[DENY CHAIN] Chain complete — resuming walk.");
                PatientUIManager.Instance.currentPatientController?.ResumeWalking();
                PatientUIManager.Instance.HideInterface();
                DialogueManager.Instance.ForceClose();
            }
        );
    }
}