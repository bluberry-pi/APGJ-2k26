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
    public void OnAdmitPressed()
    {
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

        // ✅ Count only ONCE per day
        if (controller.lastDeniedDay != currentDay)
        {
            controller.totalDenies++;
            controller.lastDeniedDay = currentDay;
        }

        // 🔹 Build ID (Day based on TOTAL denies)
        string denyID = $"onDeny_Day{controller.totalDenies}_{controller.denyCountThisVisit + 1}";
        Debug.Log("Trying sequence: " + denyID);

        DialogueSequence seq = p.dialogue?.GetSequence(denyID);

        // ❌ If no dialogue → leave
        if (seq == null)
        {
            PatientUIManager.Instance.currentPatientController?.ResumeWalking();
            PatientUIManager.Instance.HideInterface();
            return;
        }

        // ✅ Increase variation count (per visit)
        controller.denyCountThisVisit++;

        // 🚨 Mayor logic (based on TOTAL denies)
        bool isPolitician = controller.CompareTag("Politician");
        bool triggerMayorEnding = isPolitician && controller.totalDenies >= 2;

        DialogueManager.Instance.PlaySequence(
            p.dialogue,
            seq.sequenceID,
            onComplete: () =>
            {
                // 🎯 Mayor override
                if (triggerMayorEnding)
                {
                    GameOverScreen.Instance?.TriggerMayorFired();
                    return;
                }

                // 🔹 Check next variation
                string nextID = $"onDeny_Day{controller.totalDenies}_{controller.denyCountThisVisit + 1}";
                DialogueSequence nextSeq = p.dialogue?.GetSequence(nextID);

                if (nextSeq != null)
                {
                    // 👉 More dialogue exists → stay
                    return;
                }

                // 👉 No more → leave
                PatientUIManager.Instance.currentPatientController?.ResumeWalking();
                PatientUIManager.Instance.HideInterface();
            }
        );
    }
}