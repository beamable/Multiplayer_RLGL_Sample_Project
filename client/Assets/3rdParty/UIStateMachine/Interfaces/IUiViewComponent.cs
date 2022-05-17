namespace Beamable.UI
{
    public interface IUiViewComponent
    {
        string NameSpace { get; }
        string ViewName { get; }
        bool HideViewAtStart { get; }

        /// <summary>
        /// Show a view
        /// </summary>
        void Show();
        
        /// <summary>
        /// Hide a view 
        /// </summary>
        void Hide();

    }
}