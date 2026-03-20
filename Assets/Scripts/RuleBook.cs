using UnityEngine;

public class RuleBook : MonoBehaviour
{
    public GameObject rules;
    public GameObject cross;

    public void OpenRuleBook()
    {
        rules.SetActive(true);
    }

    public void CloseRuleBook()
    {
        rules.SetActive(false);
    }
}
