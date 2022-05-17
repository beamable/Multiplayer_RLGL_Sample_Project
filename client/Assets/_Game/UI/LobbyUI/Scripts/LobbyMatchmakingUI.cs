using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMatchmakingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Image ring;
    
    public void SetPlayerCount(int players)
    {
        playerCount.text = players + " / 200";
        ring.fillAmount = (float)players / 200;
    }
}