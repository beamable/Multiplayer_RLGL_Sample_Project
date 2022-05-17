using System;

namespace Beamable.Microservices
{
    [Serializable]
    public class TotalCountRequirement
    {
        public string Key;
        public int TotalCount;
    }
}