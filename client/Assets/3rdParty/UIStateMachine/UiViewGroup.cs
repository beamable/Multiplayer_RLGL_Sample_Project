using JetBrains.Annotations;
using UnityEngine;

namespace Beamable.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class UiViewGroup : MonoBehaviour
    {
        [SerializeField]
        private UiViewComponent[] views;
        [SerializeField]
        private int defaultViewIndex;
        
        private UiViewComponent _lastShownView;

        private void Start()
        {
            Show(views[defaultViewIndex]);
        }

        /// <summary>
        /// Shows a given view, then hides all other views.
        /// </summary>
        /// <param name="view">The view to be shown.</param>
        [UsedImplicitly]
        public void Show(UiViewComponent view)
        {
            if (view == _lastShownView) return;
            
            view.Show();
            
            if (_lastShownView != null)
            {
                _lastShownView.Hide();
            }
            _lastShownView = view;
        }

        /// <summary>
        /// Hides all views included in this view group.
        /// </summary>
        [UsedImplicitly]
        public void HideAll()
        {
            foreach (var view in views)
            {
                view.Hide();
            }
        }
    }
}