using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BeamableExample.RedlightGreenLight
{
    [ExecuteInEditMode]
    public class HUDAlertUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI alertText;
        // [SerializeField] private UIAnimator animator;
        public static HUDAlertUI Instance { get; private set; }
        [SerializeField] private float duration;
        public float timer = 0f;
        [SerializeField] private AnimationCurve curve;
        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            timer = duration;
        }

        private void Update()
        {
            if (timer < duration && duration > 0f)
            {
                timer += Time.deltaTime;
                alertText.transform.localScale = Vector3.one * curve.Evaluate(timer / duration);
            }
        }
        [ContextMenu("Test Animation")]
        public void TestAnimation()
        {
            DisplayAlert("Test");
        }
        public void DisplayAlert(string text)
        {
            if (alertText)
            {
                alertText.text = text;
                timer = 0f;
            }
        }
    }
}
