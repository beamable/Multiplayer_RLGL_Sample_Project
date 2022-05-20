using System;
using BeamableExample.RedlightGreenLight;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDLightUI : MonoBehaviour
{
    [SerializeField] private Color redColor;
    [SerializeField] private Color redBackgroundColor;
    [SerializeField] private Color greenColor;
    [SerializeField] private Color greenBackgroundColor;
    [Space]
    [SerializeField] private Image[] warningLightsUIBG;
    [SerializeField] private Image[] warningLightsUIFill;
    [Space]
    [SerializeField] private Image lightStatusUIBG;
    [SerializeField] private TextMeshProUGUI lightStatusText;
    [Space]
    [SerializeField] private Image[] hudDividers;

    private RedLightManager _redLightManager { get; set; }

    private void Start()
    {
        ListenToLightManager();
    }

    private void ListenToLightManager()
    {
        _redLightManager = FindObjectOfType<RedLightManager>();

        _redLightManager.GreenLightEvent += GreenLight;
        _redLightManager.RedLightEvent += RedLight;
        _redLightManager.TransitionLightEvent += TransitionLight;
    }

    private void GreenLight(object sender, EventArgs e)
    {
        lightStatusText.text = "GREEN LIGHT";
        lightStatusUIBG.color = greenBackgroundColor;
        for (var index = 0; index < warningLightsUIFill.Length; index++)
        {
            warningLightsUIBG[index].color = greenColor;
            warningLightsUIFill[index].enabled = true;
            warningLightsUIFill[index].color = greenColor;
        }

        foreach (Image divider in hudDividers)
        {
            divider.color = greenColor;
        }
    }

    private void RedLight(object sender, EventArgs e)
    {
        lightStatusText.text = "RED LIGHT";
        lightStatusUIBG.color = redBackgroundColor;
        for (var index = 0; index < warningLightsUIFill.Length; index++)
        {
            warningLightsUIBG[index].color = redColor;
            warningLightsUIFill[index].enabled = true;
            warningLightsUIFill[index].color = redColor;
        }
        foreach (Image divider in hudDividers)
        {
            divider.color = redColor;
        }
    }

    private void TransitionLight(object sender, int index)
    {
        warningLightsUIFill[index].enabled = false;
    }
}
