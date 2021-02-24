using System.Collections.Generic;

namespace RockPaperScissorsGame.Server.Models.Db
{
    public class StatisticsDb
    {
        public int UserId { get; set; }
        public Dictionary<string, GameOutcomeCounts> History { get; set; }
        public uint RockCount { get; set; }
        public uint PaperCount { get; set; }
        public uint ScissorsCount { get; set; }
        public long GameTime { get; set; }
    }
}
