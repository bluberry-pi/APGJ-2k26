using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string line;
    public Sprite portrait; // sprite shown with this line
}

[System.Serializable]
public class DialogueSequence
{
    public string sequenceID;
    public DialogueLine[] lines;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Hospital/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string personalityNote;
    public DialogueSequence[] sequences;

    public DialogueSequence GetSequence(string id)
    {
        foreach (DialogueSequence s in sequences)
            if (s.sequenceID == id) return s;
        return null;
    }
}