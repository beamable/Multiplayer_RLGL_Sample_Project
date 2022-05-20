using System.Collections;
using System.Collections.Generic;
using Beamable.Microservices;
using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementEarnedPopupUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI description;

    public async void SetUp(AchievementContent content)
    {
        icon.sprite = await content.Icon.LoadSprite();
        description.text = content.Name.ToUpper() + " : " + content.Description;
    }
}
