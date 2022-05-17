using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUsernameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;

    public void SetUsername(string alias)
    {
        username.text = alias;
    }
}
