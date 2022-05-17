using BeamableExample.Helpers;
using BeamableExample.RedlightGreenLight;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDInfoUI : SingletonNonPersistant<PlayerHUDInfoUI>
{
    [SerializeField] private Image[] weaponIcons;
    [SerializeField] private Image visibilityIcon;
    [SerializeField] private Sprite visibleSprite;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private TextMeshProUGUI numberPlayersText;
    [SerializeField] private TextMeshProUGUI winnerCircleCountText;
    [SerializeField] private Toggle[] healthIcons;

    public void UpdateHealthUI(int curHealthValue)
    {
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (i < curHealthValue)
                healthIcons[i].isOn = true;
            else
                healthIcons[i].isOn = false;
        }
    }

    public void SetWeaponIconUI(Weapon weapon)
    {
        foreach (var icon in weaponIcons)
        {
            icon.sprite = weapon.weaponUIIcon;
        }
    }

    public void SetVisibleIconUI(bool isVisible)
    {
        if (isVisible)
        {
            visibilityIcon.sprite = visibleSprite;
        }
        else
        {
            visibilityIcon.sprite = hiddenSprite;
        }
    }

    public void SetNumberPlayersText(int players)
    {
        // TODO:: We need to get the total number of players that should be connecting from Beamable.
        numberPlayersText.text = players + "/200";
    }

    public void SetWinnerCirclePlayersText(int safePlayers, int maxSafePlayers)
    {
        winnerCircleCountText.text = string.Format("{0}/{1}", safePlayers, maxSafePlayers);
    }
}
