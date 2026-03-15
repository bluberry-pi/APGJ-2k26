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

    [Header("Portrait")]
    public Image portrait;

    [Header("Buttons")]
    public Button admitButton;
    public Button denyButton;
    public Button cancelButton;

    [HideInInspector] public PatientData currentPatient;
    [HideInInspector] public PatientController currentPatientController;

    void Awake() => Instance = this;

    public void ShowPatient(PatientData data)
    {
        currentPatient = data;
        // store the controller so buttons can call ResumeWalking on the right NPC
        currentPatientController = FindCurrentPatientController();

        nameText.text = "Name: " + data.patientName;
        ageText.text = "Age: " + data.age;
        conditionText.text = "Condition: " + data.condition;
        bioText.text = "BioData: " + data.bio;
        resourceCostText.text = "Resources required: " + data.resourceCost;

        if (portrait != null && data.portrait != null)
            portrait.sprite = data.portrait;

        admitButton.interactable = HospitalManager.Instance.CanAfford(data.resourceCost);
    }

    PatientController FindCurrentPatientController()
    {
        // Finds the NPC that is currently in Waiting state
        foreach (PatientController pc in Object.FindObjectsByType<PatientController>(FindObjectsSortMode.None))
        {
            TopDownNPC npc = pc.GetComponent<TopDownNPC>();
            if (npc != null && npc.currentState == TopDownNPC.State.Waiting)
                return pc;
        }
        return null;
    }

    public void HideInterface()
    {
        yourInterface.SetActive(false);
        currentPatient = null;
        currentPatientController = null;
    }
}