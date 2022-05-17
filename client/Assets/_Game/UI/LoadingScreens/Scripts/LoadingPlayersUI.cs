using System.Collections;
using System.Collections.Generic;
using BeamableExample.Helpers;
using BeamableExample.RedlightGreenLight;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPlayersUI : SingletonNonPersistant<PlayerHUDInfoUI>
{
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Image ring;

    private void OnEnable()
    {
        SetPlayerCount();
    }

    public void SetPlayerCount()
    {
        playerCount.text = PlayerManager.allPlayers.Count + " / 200";
        ring.fillAmount = (float)PlayerManager.allPlayers.Count / 200;
    }
}
