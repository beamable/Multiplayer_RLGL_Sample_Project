using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Beamable.AccountManagement;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class ValidateInput : MonoBehaviour
{
    public UnityEvent OnValidInput;
    public UnityEvent OnInvalidInput;

    private string _passwordInput;

    [UsedImplicitly]
    public void ValidateEmail(string email)
    {
        ReportValidity(Regex.IsMatch(email, @".+@[^.@]+[\.][^.]+"));
    }

    [UsedImplicitly]
    public void ValidatePassword(string password)
    {
        ReportValidity(AccountManagementConfiguration.Instance.Overrides.IsPasswordStrong(password));
    }

    [UsedImplicitly]
    public void GetPasswordInput(string password)
    {
        _passwordInput = password;
    }

    [UsedImplicitly]
    public void ConfirmPassword(string confirm)
    {
        ReportValidity(_passwordInput == confirm);
    }

    [UsedImplicitly]
    public void ConfirmNotEmpty(string input)
    {
        ReportValidity(!string.IsNullOrEmpty(input));
    }

    private void ReportValidity(bool valid)
    {
        if (valid)
        {
            OnValidInput?.Invoke();
        }
        else
        {
            OnInvalidInput?.Invoke();
        }
    }
}
