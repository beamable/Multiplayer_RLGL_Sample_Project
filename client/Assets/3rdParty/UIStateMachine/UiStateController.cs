using Beamable.Api.Analytics;

namespace Beamable.UI
{
    using System;
    using System.Collections.Generic;

    public static class UiStateController
    {
        private static readonly List<IUiViewComponent> Views = new List<IUiViewComponent>();

        public static void RegisterView(IUiViewComponent view)
        {
            
            if (Views.FindIndex(i => i.NameSpace == view.NameSpace && i.ViewName == view.ViewName) == -1)
            {
                Views.Add(view);
            }
        }

        public static void DeregisterView(IUiViewComponent view)
        {
            if (Views.FindIndex(i => i.NameSpace == view.NameSpace && i.ViewName == view.ViewName) > -1)
            {
                Views.Remove(view);
            }
        }
        
        /// <summary>
        /// Show a view 
        /// </summary>
        /// <param name="name">Required - Name of the view</param>
        /// <param name="scope">Optional - Scope (namespace) of the view to scope to, if ommitted show all views of this name</param>
        public static void Show(string name, string scope = null)
        {
            if (string.IsNullOrEmpty(scope))
            {
                var views = Views.FindAll(i => i.ViewName == name);
                foreach (var view in views)
                {
                    view.Show();
                }
                return;
            }
            
            var viewSpecific = Views.Find(i => string.Equals(i.ViewName, name, StringComparison.CurrentCultureIgnoreCase) 
                                                && string.Equals(i.NameSpace, scope, StringComparison.CurrentCultureIgnoreCase));
            viewSpecific?.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Required - Name of the view</param>
        /// <param name="scope">Optional - Scope (namespace) of the view to scope to, if ommitted show all views of this name</param>
        public static void Hide(string name, string scope = null)
        {
            if (string.IsNullOrEmpty(scope))
            {
                var views = Views.FindAll(i => i.ViewName == name);
                foreach (var view in views)
                {
                    view.Hide();
                }
                return;
            }
            
            var viewSpecific = Views.Find(i => string.Equals(i.ViewName, name, StringComparison.CurrentCultureIgnoreCase) 
                                                && string.Equals(i.NameSpace, scope, StringComparison.CurrentCultureIgnoreCase));
            viewSpecific?.Hide();
        }
        
    }
}