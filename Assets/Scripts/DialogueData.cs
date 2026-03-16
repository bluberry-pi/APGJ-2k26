using UnityEngine;

[System.Serializable]
public class DialogueSequence
{
    public string sequenceID; // e.g. "onOpen", "onAdmit", "onDeny"
    [TextArea(2, 4)]
    public string[] lines;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Hospital/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("Personality")]
    public string personalityNote; // just for your reference e.g. "rude and impatient"

    [Header("Sequences")]
    public DialogueSequence[] sequences;

    // Finds a sequence by ID, returns null if not found
    public DialogueSequence GetSequence(string id)
    {
        foreach (DialogueSequence s in sequences)
            if (s.sequenceID == id) return s;
        return null;
    }
}