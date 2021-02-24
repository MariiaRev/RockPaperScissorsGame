using System.Collections.Generic;

namespace RockPaperScissorsGame.Server.Models.Out
{
    public class StatisticsOut
    {
        public string UserLogin { get; set; }
        public Dictionary<string, GameOutcomeCounts> History { get; set; }
        public uint RockCount { get; set; }
        public uint PaperCount { get; set; }
        public uint ScissorsCount { get; set; }
        public uint TotalRoundsCount { get; set; }
        public GameOutcomeCounts TotalOutcomesCounts { get; set; }
        public string GameTime { get; set; }
    }
}
