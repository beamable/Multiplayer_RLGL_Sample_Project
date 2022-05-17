using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ValidityMarker : MonoBehaviour
{
    [HideInInspector] public bool valid;

    [SerializeField] private ContinueRequirements continueRequirements;
    [SerializeField] private List<GameObject> successMarkers;
    [SerializeField] private List<GameObject> errorMarkers;

    [UsedImplicitly]
    public void SetValidity(bool isValid)
    {
        valid = isValid;
        ShowMarkersSuccess(valid);
        continueRequirements.CheckRequirements();
    }

    public void ShowMarkersSuccess(bool success)
    {
        foreach (var successMarker in successMarkers)
        {
            successMarker.SetActive(success);
        }
        foreach (var errorMarker in errorMarkers)
        {
            errorMarker.SetActive(!success);
        }
    }
}
