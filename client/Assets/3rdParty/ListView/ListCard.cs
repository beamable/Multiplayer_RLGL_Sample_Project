namespace ListView
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    
    public class ListCard : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public Button viewButton;
        public ListItem listItem;

        public virtual void SetTitle(string text)
        {
            if (title == null) return;
            title.text = text;
        }

        public virtual void AddButtonListener(UnityAction action)
        {
            if (viewButton == null) return;
            viewButton.onClick.AddListener(action);
        }

        public virtual void SetUp(ListItem item) { }
    }
}