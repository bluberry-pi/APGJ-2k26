using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatientUIManager : MonoBehaviour
{
    public static PatientUIManager Instance;

    [Header("The background square object")]
    public GameObject yourInterface;

    [Header("Text Fields")]
    public TextMeshProUGUI deathValueText;
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

    [HideInInspector] public PatientData currentPatient;
    [HideInInspector] public PatientController currentPatientController;

    void Awake()
    {
        Instance = this;
        Debug.Log("PatientUIManager instance on: " + gameObject.name);
    }

    public void ShowPatient(PatientData data, PatientController controller)
    {
        currentPatient = data;
        currentPatientController = controller;

        nameText.text = "Name: " + data.patientName;
        ageText.text = "Age: " + data.age;
        conditionText.text = "Condition: " + data.condition;
        bioText.text = "Bio: " + data.bio;
        resourceCostText.text = "Resources required: " + data.resourceCost;
        tierText.text = "Tier: " + ((int)data.tier + 1);
        deathValueText.text = "Death Value: " + data.deathValue;

        SetButtonsInteractable(true);

        string daySequenceID = "onOpen_Day" + (DayManager.Instance.currentDayIndex + 1);
        DialogueSequence seq = data.dialogue?.GetSequence(daySequenceID);

        if (seq == null)
            seq = data.dialogue?.GetSequence("onOpen_Day1");

        string sequenceToPlay = seq != null ? seq.sequenceID : "onOpen";

        DialogueManager.Instance.PlaySequence(data.dialogue, sequenceToPlay);
    }

    public void SetButtonsInteractable(bool state)
    {
        if (admitButton == null || denyButton == null)
        {
            Debug.LogError("Buttons not assigned in Inspector!");
            return;
        }

        // Disable everything if state is false
        if (!state)
        {
            admitButton.interactable = false;
            denyButton.interactable = false;
            return;
        }

        // Safe guards
        if (currentPatient == null)
        {
            Debug.LogError("currentPatient is NULL!");
            admitButton.interactable = false;
            denyButton.interactable = false;
            return;
        }

        if (HospitalManager.Instance == null)
        {
            Debug.LogError("HospitalManager Instance is NULL!");
            admitButton.interactable = false;
            denyButton.interactable = false;
            return;
        }

        admitButton.interactable = HospitalManager.Instance.CanAfford(currentPatient.resourceCost);
        denyButton.interactable = true;
    }

    public void HideInterface()
    {
        yourInterface.SetActive(false);
        currentPatient = null;
        currentPatientController = null;
    }
}