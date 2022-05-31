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
            var context = BeamContext.Default;
            await context.OnReady;
            var eventData = new ViewTrackingEvent(eventName, viewName, nameSpace, context.PlayerId.ToString());
            context.Api.AnalyticsTracker.TrackEvent(eventData, true);
#endif
        }
    }
}