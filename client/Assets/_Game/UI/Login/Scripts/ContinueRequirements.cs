using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class ContinueRequirements : MonoBehaviour
{
    [SerializeField] private List<ValidityMarker> requirements;

    public UnityEvent OnRequirementsMet;
    public UnityEvent OnRequirementsNotMet;

    private void OnEnable()
    {
        CheckRequirements();
    }

    public void CheckRequirements()
    {
        foreach (var requirement in requirements)
        {
            if (!requirement.valid)
            {
                OnRequirementsNotMet?.Invoke();
                return;
            }
        }
        OnRequirementsMet?.Invoke();
    }
}
