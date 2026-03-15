using UnityEngine;
using TMPro;

public class HospitalManager : MonoBehaviour
{
    public static HospitalManager Instance;

    [Header("Money")]
    public int startingMoney = 200;
    public int currentMoney;
    public TextMeshProUGUI moneyDisplay;

    [Header("Resources")]
    public int startingResources = 100;
    public int currentResources;
    public TextMeshProUGUI resourceDisplay;

    [Header("Daily Income")]
    public int resourcesPerDay = 20; // added every new day
    public int moneyPerDay = 0;      // optional, set to 0 if not needed

    void Awake()
    {
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

    // Called by DayManager at the start of each new day
    public void AddDailyIncome()
    {
        currentResources += resourcesPerDay;
        currentMoney += moneyPerDay;
        Debug.Log($"Daily income added. Resources: {currentResources}, Money: {currentMoney}");
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (moneyDisplay != null)
            moneyDisplay.text = "Money: $" + currentMoney;
        if (resourceDisplay != null)
            resourceDisplay.text = "Resources: " + currentResources;
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

        currentResources -= p.resourceCost;
        AddMoney(p.rewardMoney);

        GameObject patientObj = PatientUIManager.Instance.currentPatientController.gameObject;
        DayManager.Instance.RemovePatient(patientObj);
        DayManager.Instance.PatientAdmitted();

        PatientUIManager.Instance.HideInterface();
        Destroy(patientObj);

        Debug.Log($"Admitted {p.patientName}. Earned ${p.rewardMoney}. Resources left: {currentResources}");
    }

    // =============================================
    // DENY BUTTON
    // =============================================
    public void OnDenyPressed()
    {
        PatientUIManager.Instance.currentPatientController?.ResumeWalking();
        PatientUIManager.Instance.HideInterface();
        Debug.Log("Patient denied.");
    }

    // =============================================
    // CANCEL BUTTON
    // =============================================
    public void OnCancelPressed()
    {
        PatientUIManager.Instance.currentPatientController?.ResumeWalking();
        PatientUIManager.Instance.HideInterface();
        Debug.Log("Cancelled.");
    }
}