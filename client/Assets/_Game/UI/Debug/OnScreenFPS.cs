using System.Collections;
using TMPro;
using UnityEngine;

namespace _Game.UI.Debug
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class OnScreenFPS : MonoBehaviour
    {
        [SerializeField]
        private float interval;
        private WaitForSeconds updateDelay;
        
        private TextMeshProUGUI _tmp;
        private bool running;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            updateDelay = new WaitForSeconds(interval);
            running = true;
            StartCoroutine(UpdateAfterInterval());
        }
    
        private void UpdateCounter()
        {
            _tmp.text = $"{(int)(1f / Time.deltaTime)} FPS";
        }

        private IEnumerator UpdateAfterInterval()
        {
            while (running)
            {
                UpdateCounter();
                yield return updateDelay;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            running = false;
        }
    }
}
