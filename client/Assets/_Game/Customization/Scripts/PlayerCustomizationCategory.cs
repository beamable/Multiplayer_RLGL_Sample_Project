using System;
using Beamable.Common.Api.Inventory;
using Beamable.Stats;
using UnityEngine.Events;

namespace _Game.Features.Customization.Scripts
{
    [Serializable]
    public class PlayerCustomizationCategory
    {
        public string id;
        public StatObject categoryStat;
        public UnityEvent<string> OnStatChanged;
    }
}