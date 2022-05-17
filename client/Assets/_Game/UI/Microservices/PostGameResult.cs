namespace Beamable.Microservices
{
   using System;
    using System.Collections.Generic;

    [Serializable]
    public class PostGameResult
    {
        public long PlayerId;
        public int GamesPlayed;
        public int Score;
        public int OldRank;
        public int NewRank;
        public List<Reward> Rewards = new List<Reward>();
        public string errorMessage;
        public string errorStack;
    }
}