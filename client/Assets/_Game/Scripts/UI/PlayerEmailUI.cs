using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using TMPro;
using UnityEngine;

public class PlayerEmailUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI emailText;

    private BeamContext _context;

    private void OnEnable()
    {
        SetEmailUI();
    }

    private async Task SetUpBeamable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
    }

    private async void SetEmailUI()
    {
        await SetUpBeamable();
        if (_context.Api.User.HasDBCredentials())
        {
            emailText.text = _context.Api.User.email;
        }
    }

}
