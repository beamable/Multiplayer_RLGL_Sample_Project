using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ManualMatchHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField customMatchInput;

    [SerializeField]
    private UnityEvent<string> OnCustomMatchStart;

    public void StartMatch()
    {
        OnCustomMatchStart?.Invoke(customMatchInput.text);
    }
}