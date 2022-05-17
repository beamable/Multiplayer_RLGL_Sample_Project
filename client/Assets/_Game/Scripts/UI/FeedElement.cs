using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class FeedElement : MonoBehaviour
    {
        public TextMeshProUGUI feedText;

        public Action<FeedElement> OnTimerExpiredCallback;

        public void Initialize(float expirationTime)
        {
            StartCoroutine(TimerLoop(expirationTime));
        }

        private IEnumerator TimerLoop(float expirationTime)
        {
            yield return new WaitForSeconds(expirationTime);

            OnTimerExpiredCallback?.Invoke(this);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
