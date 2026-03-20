using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatientUIManager : MonoBehaviour
{
    public static PatientUIManager Instance;

    [Header("The background square object")]
    public GameObject yourInterface;

    [Header("Text Fields")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI conditionText;
    public TextMeshProUGUI bioText;
    public TextMeshProUGUI resourceCostText;
    public TextMeshProUGUI tierText;

    [Header("Portrait")]
    public Image portrait;

    [Header("Buttons")]
    public Button admitButton;
    public Button denyButton;
    //public Button cancelButton;

    [HideInInspector] public PatientData currentPatient;
    [HideInInspector] public PatientController currentPatientController;

    void Awake() => Instance = this;

    public void ShowPatient(PatientData data)
    {
        currentPatient = data;
        currentPatientController = FindCurrentPatientController();

        nameText.text = "Name: " + data.patientName;
        ageText.text = "Age: " + data.age;
        conditionText.text = "Condition: " + data.condition;
        bioText.text = "Bio: " + data.bio;
        resourceCostText.text = "Resources required: " + data.resourceCost;
        tierText.text = "Tier: " + ((int)data.tier + 1).ToString();

        // portrait removed — handled by DialogueManager per line

        SetButtonsInteractable(false);

        string daySequenceID = "onOpen_Day" + (DayManager.Instance.currentDayIndex + 1);
        DialogueSequence seq = data.dialogue?.GetSequence(daySequenceID);
        string sequenceToPlay = seq != null ? daySequenceID : "onOpen";

        DialogueManager.Instance.PlaySequence(
            data.dialogue,
            sequenceToPlay,
            onComplete: () => SetButtonsInteractable(true)
        );
    }

    public void SetButtonsInteractable(bool state)
    {
        admitButton.interactable = true && HospitalManager.Instance.CanAfford(currentPatient.resourceCost);
        denyButton.interactable = true;
        //cancelButton.interactable = true;
    }

    public void HideInterface()
    {
        yourInterface.SetActive(false);
        currentPatient = null;
        currentPatientController = null;
    }

    PatientController FindCurrentPatientController()
    {
        foreach (PatientController pc in Object.FindObjectsByType<PatientController>(FindObjectsSortMode.None))
        {
            TopDownNPC npc = pc.GetComponent<TopDownNPC>();
            if (npc != null && npc.currentState == TopDownNPC.State.Waiting)
                return pc;
        }
        return null;
    }
}