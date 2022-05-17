using System;
using BeamableExample.RedlightGreenLight;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDLightUI : MonoBehaviour
{
    [SerializeField] private Sprite greenRadialBG;
    [SerializeField] private Sprite greenRadialFill;
    [Space]
    [SerializeField] private Sprite redRadialBG;
    [SerializeField] private Sprite redRadialFill;
    [Space]
    [SerializeField] private Image[] warningLightsUIBG;
    [SerializeField] private Image[] warningLightsUIFill;
    [Space]
    [SerializeField] private Sprite greenLightUIBG;
    [SerializeField] private Sprite redLightUIBG;
    [SerializeField] private Image lightStatusUIBG;
    [Space]
    [SerializeField] private TextMeshProUGUI lightStatusText;
    [Space]
    [SerializeField] private Image[] hudDividers;
    [SerializeField] private Sprite greenDivider;
    [SerializeField] private Sprite redDivider;

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
        lightStatusUIBG.sprite = greenLightUIBG;
        for (var index = 0; index < warningLightsUIFill.Length; index++)
        {
            warningLightsUIBG[index].sprite = greenRadialBG;
            warningLightsUIFill[index].enabled = true;
            warningLightsUIFill[index].sprite = greenRadialFill;
        }

        foreach (Image divider in hudDividers)
        {
            divider.sprite = greenDivider;
        }
    }

    private void RedLight(object sender, EventArgs e)
    {
        lightStatusText.text = "RED LIGHT";
        lightStatusUIBG.sprite = redLightUIBG;
        for (var index = 0; index < warningLightsUIFill.Length; index++)
        {
            warningLightsUIBG[index].sprite = redRadialBG;
            warningLightsUIFill[index].enabled = true;
            warningLightsUIFill[index].sprite = redRadialFill;
        }
        foreach (Image divider in hudDividers)
        {
            divider.sprite = redDivider;
        }
    }

    private void TransitionLight(object sender, int index)
    {
        warningLightsUIFill[index].enabled = false;
    }
}
