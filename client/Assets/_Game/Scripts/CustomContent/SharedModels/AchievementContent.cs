using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using UnityEngine.AddressableAssets;

namespace Beamable.Microservices
{
    [ContentType("achievements")]
    public class AchievementContent : ContentObject
    {
        public string Name;
        public string Description;
        public AssetReferenceSprite Icon;
        public List<StringRequirement> StringRequirements;
        public TotalCountRequirement TotalCountRequirement;
        public bool IsSecret;
    }
    
    [Serializable]
    public class AchievementRef : ContentRef<AchievementContent> { }
    [Serializable]
    public class AchievementLink : ContentLink<AchievementContent> { }
}