using UnityEngine;
using TMPro;

public class FamilyManager : MonoBehaviour
{
    public static FamilyManager Instance;

    [Header("Rations")]
    public int dailyRationCost = 20;

    [Header("Sickness")]
    [Range(0f, 1f)]
    public float sicknessProbability = 0.2f; // 20% chance per member per day
    public int medicineCost = 30;

    [Header("Family Members")]
    public FamilyMember wife;
    public FamilyMember son;
    public FamilyMember daughter;

    [Header("UI")]
    public TextMeshProUGUI wifeText;
    public TextMeshProUGUI sonText;
    public TextMeshProUGUI daughterText;

    void Awake() => Instance = this;

    void Start() => UpdateFamilyUI();

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
            Debug.Log("Cannot afford rations!");
        }

        // Random sickness
        RollSickness(wife);
        RollSickness(son);
        RollSickness(daughter);

        UpdateFamilyUI();
    }

    void RollSickness(FamilyMember member)
    {
        if (member.isDead) return;
        if (!member.isSick && Random.value < sicknessProbability)
        {
            member.isSick = true;
            Debug.Log($"{member.memberName} got sick!");
        }
    }

    // Call this from a UI button — "Send Medicine"
    public void SendMedicine(string memberName)
    {
        FamilyMember member = GetMember(memberName);
        if (member == null || !member.isSick) return;

        if (HospitalManager.Instance.CanAfford(medicineCost))
        {
            HospitalManager.Instance.SpendMoney(medicineCost);
            member.isSick = false;
            Debug.Log($"{member.memberName} cured.");
        }
        else
        {
            Debug.Log("Cannot afford medicine!");
        }

        UpdateFamilyUI();
    }

    // Call at end of day if medicine not sent
    public void CheckUntreatedSickness()
    {
        CheckDeath(wife);
        CheckDeath(son);
        CheckDeath(daughter);
        UpdateFamilyUI();
    }

    void CheckDeath(FamilyMember member)
    {
        if (member.isSick && !member.isDead)
        {
            member.isDead = true;
            Debug.Log($"{member.memberName} has died.");
            // Add consequences here later
        }
    }

    FamilyMember GetMember(string name)
    {
        if (wife.memberName == name) return wife;
        if (son.memberName == name) return son;
        if (daughter.memberName == name) return daughter;
        return null;
    }

    void UpdateFamilyUI()
    {
        wifeText.text = GetStatusText(wife);
        sonText.text = GetStatusText(son);
        daughterText.text = GetStatusText(daughter);
    }

    string GetStatusText(FamilyMember m)
    {
        if (m.isDead) return m.memberName + " (Dead)";
        if (m.isSick) return m.memberName + " (Sick)";
        return m.memberName;
    }
}

[System.Serializable]
public class FamilyMember
{
    public string memberName;
    public bool isSick = false;
    public bool isDead = false;
}