using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FamilyManager : MonoBehaviour
{
    public static FamilyManager Instance;

    [Header("Rations")]
    public int dailyRationCost = 20;

    [Header("Medicine")]
    public int medicineCostPerPerson = 5;

    [Header("Health Drain")]
    public float healthDrainPerSecond = 1f; // how fast health drains while playing

    [Header("Medicine Heal Amount")]
    public int healAmount = 20; // how much health restored per med send

    [Header("Family Members")]
    public FamilyMember wife;
    public FamilyMember son;
    public FamilyMember daughter;

    [Header("Family Icon UI")]
    public GameObject familyIconButton;
    public GameObject exclamationMark;

    [Header("Family Panel UI")]
    public GameObject familyPanel;

    // Wife UI
    public Slider wifeHealthBar;
    public TextMeshProUGUI wifeNameText;
    public Button wifeMedsButton;

    // Son UI
    public Slider sonHealthBar;
    public TextMeshProUGUI sonNameText;
    public Button sonMedsButton;

    // Daughter UI
    public Slider daughterHealthBar;
    public TextMeshProUGUI daughterNameText;
    public Button daughterMedsButton;

    private bool gameOver = false;

    void Awake() => Instance = this;

    void Start()
    {
        familyPanel.SetActive(false);
        exclamationMark.SetActive(false);

        // Init sliders
        InitMember(wife, wifeHealthBar, wifeNameText);
        InitMember(son, sonHealthBar, sonNameText);
        InitMember(daughter, daughterHealthBar, daughterNameText);
    }

    void InitMember(FamilyMember member, Slider bar, TextMeshProUGUI nameText)
    {
        member.currentHealth = member.maxHealth;
        bar.maxValue = member.maxHealth;
        bar.value = member.currentHealth;
        nameText.text = member.memberName;
    }

    void Update()
    {
        if (gameOver) return;

        // Drain health every frame
        DrainHealth(wife, wifeHealthBar);
        DrainHealth(son, sonHealthBar);
        DrainHealth(daughter, daughterHealthBar);

        // Check deaths
        CheckDeath(wife);
        CheckDeath(son);
        CheckDeath(daughter);

        // Update exclamation — show when anyone is low
        UpdateExclamation();
    }

    void DrainHealth(FamilyMember member, Slider bar)
    {
        if (member.isDead) return;
        member.currentHealth -= healthDrainPerSecond * Time.deltaTime;
        member.currentHealth = Mathf.Clamp(member.currentHealth, 0, member.maxHealth);
        bar.value = member.currentHealth;
    }

    void CheckDeath(FamilyMember member)
    {
        if (member.isDead) return;
        if (member.currentHealth <= 0)
        {
            member.isDead = true;
            Debug.Log($"{member.memberName} has died. GAME OVER.");
            gameOver = true;
            // TODO: trigger game over screen here
        }
    }

    void UpdateExclamation()
    {
        bool anyoneLow = (wife.currentHealth < wife.maxHealth * 0.3f && !wife.isDead) ||
                         (son.currentHealth < son.maxHealth * 0.3f && !son.isDead) ||
                         (daughter.currentHealth < daughter.maxHealth * 0.3f && !daughter.isDead);
        exclamationMark.SetActive(anyoneLow);
    }

    // =============================================
    // FAMILY ICON BUTTON
    // =============================================
    public void OnFamilyIconPressed()
    {
        familyPanel.SetActive(!familyPanel.activeSelf);
    }

    // =============================================
    // SEND MEDS BUTTONS — one per member
    // =============================================
    public void SendMedsWife()     => SendMeds(wife, wifeHealthBar);
    public void SendMedsSon()      => SendMeds(son, sonHealthBar);
    public void SendMedsDaughter() => SendMeds(daughter, daughterHealthBar);

    void SendMeds(FamilyMember member, Slider bar)
    {
        if (member.isDead) return;

        if (!HospitalManager.Instance.CanAfford(medicineCostPerPerson))
        {
            Debug.Log("Not enough money for medicine.");
            return;
        }

        HospitalManager.Instance.SpendMoney(medicineCostPerPerson);
        member.currentHealth = Mathf.Clamp(member.currentHealth + healAmount, 0, member.maxHealth);
        bar.value = member.currentHealth;

        Debug.Log($"Sent meds to {member.memberName}. Health: {member.currentHealth}");
    }

    // =============================================
    // END OF DAY — deduct rations
    // =============================================
    public void EndOfDayUpdate()
    {
        if (HospitalManager.Instance.CanAfford(dailyRationCost))
        {
            HospitalManager.Instance.SpendMoney(dailyRationCost);
            Debug.Log("Rations sent.");
        }
        else
        {
            Debug.Log("Cannot afford rations.");
        }
    }
}

[System.Serializable]
public class FamilyMember
{
    public string memberName;
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool isDead = false;
}