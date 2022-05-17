namespace Beamable.UI
{
    using System.Collections.Generic;
    using Beamable.Api.Analytics;
    
    public class ViewTrackingEvent: CoreEvent
    {
        public ViewTrackingEvent(string eventName, string viewName, string viewScope, string dbid) : base(
            "ui_event", 
            "ui_trace", 
            new Dictionary<string, object>()
            {
                ["eventName"] = eventName,
                ["name"] = viewName,
                ["scope"] = viewScope,
                ["dbid"] = dbid
            }){}
    }
}