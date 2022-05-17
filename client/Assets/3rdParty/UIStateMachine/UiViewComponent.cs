using System;

namespace Beamable.UI
{
   using UnityEngine;
   using UnityEngine.Events;

   public class UiViewComponent : MonoBehaviour, IUiViewComponent
    {
        public string NameSpace => nameSpace;
        public string ViewName => viewName;
        public bool HideViewAtStart => hideViewAtStart;
       
        public UnityEvent OnViewShown = new UnityEvent();
        public UnityEvent OnViewHidden = new UnityEvent();

        [SerializeField] private string nameSpace;
        [SerializeField] private string viewName;
        [SerializeField] private bool hideViewAtStart;


        private void Awake()
        {
            UiStateController.RegisterView(this);
        }

        private void OnDestroy()
        {
            UiStateController.DeregisterView(this);
        }

        private void Start()
        {
            if (hideViewAtStart)
            {
                Hide();
                return;
            }

            Show();
        }

        public void Show()
        {
            OnViewShown?.Invoke();
            TrackEvent("show_view");
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            OnViewHidden?.Invoke();
            TrackEvent("hide_view");
            gameObject.SetActive(false);
        }

        private async void TrackEvent(string eventName)
        {
#if BEAMABLE_UI_ANALYTICS
            var beamableAPI = await Beamable.API.Instance;
            var eventData = new ViewTrackingEvent(eventName, viewName, nameSpace, beamableAPI.User.id.ToString());
            beamableAPI.AnalyticsTracker.TrackEvent(eventData, true);
#endif
        }
    }
}