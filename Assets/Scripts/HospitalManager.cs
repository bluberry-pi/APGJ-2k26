using System.Collections;
using UnityEngine;
using TMPro;

public class HospitalManager : MonoBehaviour
{
    [Header("Admit Settings")]
    public static HospitalManager Instance;

    [Header("Money")]
    public int startingMoney = 200;
    public int currentMoney;
    public TextMeshProUGUI moneyDisplay;

    [Header("Res")]
    public int startingRes = 100;
    public int currentRes;
    public TextMeshProUGUI resourceDisplay;

    [Header("Daily Income")]
    public int ResPerDay = 20; // added every new day
    public int moneyPerDay = 0;      // optional, set to 0 if not needed

    void Awake()
    {
        Instance = this;
        currentMoney = startingMoney;
        currentRes = startingRes;
    }

    void Start() => UpdateDisplay();

    public bool CanAfford(int cost) => currentRes >= cost;

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

    // Called by DayManager at the start of each new day
    public void AddDailyIncome()
    {
        currentRes += ResPerDay;
        currentMoney += moneyPerDay;
        Debug.Log($"Daily income added. Res: {currentRes}, Money: {currentMoney}");
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (moneyDisplay != null)
            moneyDisplay.text = "Money: $" + currentMoney;
        if (resourceDisplay != null)
            resourceDisplay.text = "Res: " + currentRes;
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
            Debug.Log("Not enough Res.");
            return;
        }

        // Lock buttons so player cant press again
        PatientUIManager.Instance.SetButtonsInteractable(false);

        // Play onAdmit dialogue, THEN complete the admit after delay
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
        currentRes -= p.resourceCost;
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
    // DENY BUTTON — patient argues, may need multiple presses
    // =============================================
    private int denyPressCount = 0;

    public void OnDenyPressed()
    {
        PatientData p = PatientUIManager.Instance.currentPatient;
        if (p == null) return;

        PatientUIManager.Instance.SetButtonsInteractable(false);

        string day = (DayManager.Instance.currentDayIndex + 1).ToString();

        // Build day-specific IDs first, fall back to generic
        string denyID = denyPressCount == 0 ? "onDeny" : "onDenyFinal";
        string dayDenyID = denyPressCount == 0 ? "onDeny_Day" + day : "onDenyFinal_Day" + day;

        // Check if day-specific version exists, use it, otherwise use generic
        DialogueSequence seq = p.dialogue?.GetSequence(dayDenyID);
        string sequenceToPlay = seq != null ? dayDenyID : denyID;

        denyPressCount++;

        DialogueManager.Instance.PlaySequence(
            p.dialogue,
            sequenceToPlay,
            onComplete: () =>
            {
                if (denyID == "onDenyFinal")
                {
                    denyPressCount = 0;
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

    // =============================================
    // CANCEL BUTTON
    // =============================================
    public void OnCancelPressed()
    {
        denyPressCount = 0;
        PatientUIManager.Instance.currentPatientController?.ResumeWalking();
        PatientUIManager.Instance.HideInterface();
        Debug.Log("Cancelled.");
    }
}