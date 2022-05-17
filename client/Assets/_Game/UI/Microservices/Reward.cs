namespace Beamable.Microservices
{
    using System;
    [Serializable]
    public class Reward
    {
        public bool IsItem = true;
        public string CurrencyType;
        public int Amount;
        public string ItemContentId;
    }
}