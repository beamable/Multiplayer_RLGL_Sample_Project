using BeamableExample.RedlightGreenLight;
using TMPro;
using UnityEngine;

public class HUDTimerUI : MonoBehaviour
{
    private MatchTimer _matchTimer;
    [SerializeField] private TextMeshProUGUI timerText;

    public void SetTimerText(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
