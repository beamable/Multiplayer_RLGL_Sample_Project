using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace BeamableExample.RedlightGreenLight
{
    [RequireComponent(typeof(RawImage), typeof(EventTrigger))]
    public class InteractiveLobbyCharacterUI : MonoBehaviour
    {
        private RawImage image;
        private EventTrigger eventTrigger;
        private InteractiveLobbyCharacter lobbyCharacterController;
        private Vector2 mousePos;
        private bool IsDragging = false;
        void OnEnable()
        {
            if (TryGetComponent<RawImage>(out image) && TryGetComponent<EventTrigger>(out eventTrigger))
            {
                eventTrigger.triggers.RemoveRange(0, eventTrigger.triggers.Count);

                AddEventTriggerListener(eventTrigger, EventTriggerType.PointerDown, OnPointerDown);
                AddEventTriggerListener(eventTrigger, EventTriggerType.PointerUp, OnPointerUp);
                AddEventTriggerListener(eventTrigger, EventTriggerType.PointerExit, OnPointerUp);
            }

            lobbyCharacterController = FindObjectOfType<InteractiveLobbyCharacter>();
        }

        private void Update()
        {
            if (IsDragging)
            {
                OnDrag();
            }
        }
        public void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
            trigger.triggers.Add(entry);
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            mousePos = Mouse.current.position.ReadValue();
            IsDragging = true;
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            mousePos = Mouse.current.position.ReadValue();
            OnDrag();
            IsDragging = false;
        }

        public void OnDrag()
        {
            Vector2 newMousePos = Mouse.current.position.ReadValue();
            Vector2 deltaPos = newMousePos - mousePos;

            if (lobbyCharacterController)
                lobbyCharacterController.RotateCharacter(-deltaPos.x);

            mousePos = newMousePos;
        }
    }
}