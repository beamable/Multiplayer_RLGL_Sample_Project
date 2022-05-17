using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class SetTextFromInput : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TMP_InputField input;

    [UsedImplicitly]
    public void SetTextToInput()
    {
        text.text = input.text;
    }
    
    [UsedImplicitly]
    public void AddInputToText()
    {
        text.text += input.text;
    }
}
