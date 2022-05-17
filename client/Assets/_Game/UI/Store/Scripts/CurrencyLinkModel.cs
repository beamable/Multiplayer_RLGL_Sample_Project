using UnityEngine.Events;

namespace _Game.UI.Store.Scripts
{
    [System.Serializable]
    public class CurrencyUpdatedModel
    {
        public string id;
        public UnityEvent<int> onChangedEvent;
    }
}
