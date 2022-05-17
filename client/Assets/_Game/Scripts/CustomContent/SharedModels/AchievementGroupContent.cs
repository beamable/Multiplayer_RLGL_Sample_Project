using System;
using System.Collections.Generic;
using Beamable.Common.Content;

namespace Beamable.Microservices
{
    [ContentType("achievement_group")]
    public class AchievementGroupContent : ContentObject
    {
        public List<AchievementRef> achievements;
    }
    
    [Serializable]
    public class AchievementGroupRef : ContentRef<AchievementGroupContent> { }
    [Serializable]
    public class AchievementGroupLink : ContentLink<AchievementGroupContent> { }
}
