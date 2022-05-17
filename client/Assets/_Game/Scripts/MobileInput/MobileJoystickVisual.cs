using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace _Game.Scripts.UI
{
    public class MobileJoystickVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private RectTransform handle;
        [SerializeField]
        private RectTransform center;
        [SerializeField]
        private RectTransform arrow;

        private void Awake()
        {
            arrow.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateArrowRotation();
        }
        
        private void UpdateArrowRotation()
        {
            var zAngle = CalculateArrowRotation();
            arrow.rotation = Quaternion.Euler(0, 0, zAngle);
        }

        private float CalculateArrowRotation()
        {
            var difference = (handle.anchoredPosition - center.anchoredPosition).normalized;
            return Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            arrow.gameObject.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            arrow.gameObject.SetActive(false);
        }
    }
}