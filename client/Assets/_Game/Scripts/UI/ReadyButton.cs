using BeamableExample.RedlightGreenLight;
using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Scripts.UI
{
    public class ReadyButton : MonoBehaviour
    {
        [UsedImplicitly]
        public void SetReady()
        {
            PlayerInputListener.Instance.Ready = true;
        }
    }
}
