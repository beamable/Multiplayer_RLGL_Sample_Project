using TMPro;
using UnityEngine;

namespace _Game.Features.Matchmaking
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DebugMatchId : MonoBehaviour
    {
        [SerializeField]
        private MatchConfig matchConfig;

        private TextMeshProUGUI matchIdTMP;
    
        void Start()
        {
            if (matchConfig == null) return;
            matchIdTMP = GetComponent<TextMeshProUGUI>();
            if (matchIdTMP == null) return;
            matchIdTMP.text = matchConfig.MatchId;
        }
    }
}