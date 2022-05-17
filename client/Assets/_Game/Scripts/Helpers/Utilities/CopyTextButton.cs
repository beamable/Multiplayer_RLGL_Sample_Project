using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CopyTextButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textToCopy;
    
    public void CopyToClipboard()
    {
        if (textToCopy == null) return;
        var te = new TextEditor
        {
            text = textToCopy.text
        };
        te.SelectAll();
        te.Copy();
    }
}