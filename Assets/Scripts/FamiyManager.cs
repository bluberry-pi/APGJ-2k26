using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FamilyManager : MonoBehaviour
{
    public static FamilyManager Instance;

    [Header("Rations")]
    public int dailyRationCost = 20;

    [Header("Medicine")]
    public int medicineCostPerPerson = 10;

    [Header("Sickness Probability")]
    [Range(0f, 1f)]
    public float sicknessProbability = 0.2f;

    [Header("Family Members")]
    public FamilyMember wife;
    public FamilyMember son;
    public FamilyMember daughter;

    [Header("Family Icon UI")]
    public GameObject familyIconButton;      // the bottom left icon button
    public GameObject exclamationMark;       // red ! on the icon

    [Header("Family Panel UI")]
    public GameObject familyPanel;           // the paper panel
    public TextMeshProUGUI wifeStatusText;
    public TextMeshProUGUI sonStatusText;
    public TextMeshProUGUI daughterStatusText;
    public Button sendMedsButton;
    public TextMeshProUGUI sendMedsCostText;

    void Awake() => Instance = this;

    void Start()
    {
        familyPanel.SetActive(false);
        exclamationMark.SetActive(false);
        UpdateFamilyUI();
    }

    // =============================================
    // Called every end of day by DayManager
    // =============================================
    public void EndOfDayUpdate()
    {
        // Pay rations
        if (HospitalManager.Instance.CanAfford(dailyRationCost))
        {
            HospitalManager.Instance.SpendMoney(dailyRationCost);
            Debug.Log("Rations sent.");
        }
        else
        {
            Debug.Log("Cannot afford rations.");
        }

        // Roll random sickness only for members not already sick
        // and not manually overridden this day
        RollSickness(wife);
        RollSickness(son);
        RollSickness(daughter);

        // Check if anyone died from untreated sickness
        CheckDeath(wife);
        CheckDeath(son);
        CheckDeath(daughter);

        UpdateFamilyUI();
        UpdateExclamation();
    }

    // =============================================
    // Manual override — call from DayData or inspector
    // set a member sick on a specific day
    // =============================================
    public void ForceSetSick(string memberName, bool sick)
    {
        FamilyMember m = GetMember(memberName);
        if (m == null || m.isDead) return;
        m.isSick = sick;
        m.manuallySetThisDay = true; // prevents random roll overriding it
        UpdateFamilyUI();
        UpdateExclamation();
    }

    void RollSickness(FamilyMember member)
    {
        if (member.isDead) return;
        if (member.manuallySetThisDay)
        {
            member.manuallySetThisDay = false; // reset for next day
            return;
        }
        if (!member.isSick && Random.value < sicknessProbability)
        {
            member.isSick = true;
            Debug.Log($"{member.memberName} got sick!");
        }
    }

    void CheckDeath(FamilyMember member)
    {
        if (member.isSick && !member.isDead && member.daysUntreated >= 1)
        {
            member.isDead = true;
            Debug.Log($"{member.memberName} has died.");
            // add consequences here later
        }
        else if (member.isSick && !member.isDead)
        {
            member.daysUntreated++;
        }
    }

    // =============================================
    // FAMILY ICON BUTTON — opens/closes panel
    // =============================================
    public void OnFamilyIconPressed()
    {
        bool isOpen = familyPanel.activeSelf;
        familyPanel.SetActive(!isOpen);
        UpdateFamilyUI();
    }

    // =============================================
    // SEND MEDS BUTTON
    // =============================================
    public void OnSendMedsPressed()
    {
        int totalCost = 0;

        if (wife.isSick && !wife.isDead)     totalCost += medicineCostPerPerson;
        if (son.isSick && !son.isDead)       totalCost += medicineCostPerPerson;
        if (daughter.isSick && !daughter.isDead) totalCost += medicineCostPerPerson;

        if (!HospitalManager.Instance.CanAfford(totalCost))
        {
            Debug.Log("Cannot afford medicine.");
            return;
        }

        HospitalManager.Instance.SpendMoney(totalCost);

        if (wife.isSick)     { wife.isSick = false;     wife.daysUntreated = 0; }
        if (son.isSick)      { son.isSick = false;      son.daysUntreated = 0; }
        if (daughter.isSick) { daughter.isSick = false; daughter.daysUntreated = 0; }

        Debug.Log($"Medicine sent. Cost: ${totalCost}");

        UpdateFamilyUI();
        UpdateExclamation();
    }

    void UpdateExclamation()
    {
        bool anyoneSick = (wife.isSick && !wife.isDead) ||
                          (son.isSick && !son.isDead) ||
                          (daughter.isSick && !daughter.isDead);
        exclamationMark.SetActive(anyoneSick);
    }

    void UpdateFamilyUI()
    {
        wifeStatusText.text     = GetStatusText(wife);
        sonStatusText.text      = GetStatusText(son);
        daughterStatusText.text = GetStatusText(daughter);

        // Send meds button only active if someone is sick and alive
        bool anyoneSick = (wife.isSick && !wife.isDead) ||
                          (son.isSick && !son.isDead) ||
                          (daughter.isSick && !daughter.isDead);

        sendMedsButton.interactable = anyoneSick;

        // Show total medicine cost
        int totalCost = 0;
        if (wife.isSick && !wife.isDead)         totalCost += medicineCostPerPerson;
        if (son.isSick && !son.isDead)           totalCost += medicineCostPerPerson;
        if (daughter.isSick && !daughter.isDead) totalCost += medicineCostPerPerson;

        sendMedsCostText.text = totalCost > 0 ? "Send Meds: $" + totalCost : "No meds needed";
    }

    string GetStatusText(FamilyMember m)
    {
        if (m.isDead)  return m.memberName + " — Deceased";
        if (m.isSick)  return m.memberName + " — Sick";
        return m.memberName + " — OK";
    }

    FamilyMember GetMember(string name)
    {
        if (wife.memberName == name)      return wife;
        if (son.memberName == name)       return son;
        if (daughter.memberName == name)  return daughter;
        return null;
    }
}

[System.Serializable]
public class FamilyMember
{
    public string memberName;
    public bool isSick = false;
    public bool isDead = false;
    public int daysUntreated = 0;
    [HideInInspector] public bool manuallySetThisDay = false;
}