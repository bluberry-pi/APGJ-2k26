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

        PatientUIManager.Instance.SetButtonsInteractable(false);
        string denyID = "onDeny_Day" + (controller.denyCount + 1);
        DialogueSequence seq = p.dialogue?.GetSequence(denyID);

        if (seq == null)
            seq = p.dialogue?.GetSequence("onDeny_Day1");

        string sequenceToPlay = seq != null ? seq.sequenceID : "onDeny";

        controller.denyCount++;
        DialogueManager.Instance.PlaySequence(
            p.dialogue,
            sequenceToPlay,
            onComplete: () =>
            {
                // If you reach a "final" deny (optional)
                if (sequenceToPlay == "onDenyFinal")
                {
                    controller.denyCount = 0;
                    PatientUIManager.Instance.currentPatientController?.ResumeWalking();
                    PatientUIManager.Instance.HideInterface();
                }
                else
                {
                    PatientUIManager.Instance.SetButtonsInteractable(true);
                }
            }
        );
    }
}