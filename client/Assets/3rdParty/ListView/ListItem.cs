namespace ListView
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    
    public class ListViewData : List<ListItem> { }
    public class ListItem
    {
        public int ListPrefabIndex = 0;
        public string Id;
        public string Title;
        public Dictionary<string,object> PropertyBag = new Dictionary<string, object>();
        public UnityAction ViewAction;
    }
}