using UnityEngine;

public class CounterPosition : MonoBehaviour
{
    public static CounterPosition Instance;
    void Awake() => Instance = this;
}