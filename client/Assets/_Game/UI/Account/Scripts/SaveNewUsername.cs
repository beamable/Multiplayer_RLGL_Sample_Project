using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SaveNewUsername : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TextMeshProUGUI nameText;

    public UnityEvent<string> OnNameSave;

    private void OnEnable()
    {
        input.text = nameText.text;
    }

    public void SaveInput()
    {
        OnNameSave?.Invoke(input.text);
    }
}
