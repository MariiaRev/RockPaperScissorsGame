using System.Collections.Generic;
using System.Text;

namespace RockPaperScissorsGame.Client.Models
{
    public class UserStatistics
    {
        public string UserLogin { get; set; }
        public Dictionary<string, GameOutcomeCounts> History { get; set; }
        public uint RockCount { get; set; }
        public uint PaperCount { get; set; }
        public uint ScissorsCount { get; set; }
        public uint TotalRoundsCount { get; set; }
        public GameOutcomeCounts TotalOutcomesCounts { get; set; }
        public string GameTime { get; set; }

        public override string ToString()
        {
            var history = new StringBuilder($"\n{" ",4}History:");

            foreach (var day in History)
            {
                history.Append($"\n{" ",8}{day.Key}");
                history.Append($"\n{" ",12}  Wins count: {day.Value.WinsCount}");
                history.Append($"\n{" ",12}Losses count: {day.Value.LossesCount}");
                history.Append($"\n{" ",12} Draws count: {day.Value.DrawsCount}");
            }

            var counts = new StringBuilder();
            counts.Append($"\n\n{" ",4}    Rock count: {RockCount}");
            counts.Append($"\n{" ",4}   Paper count: {PaperCount}");
            counts.Append($"\n{" ",4}Scissors count: {ScissorsCount}");
            counts.Append($"\n\n{" ",4}  Total wins count: {TotalOutcomesCounts.WinsCount}");
            counts.Append($"\n{" ",4}Total losses count: {TotalOutcomesCounts.LossesCount}");
            counts.Append($"\n{" ",4} Total draws count: {TotalOutcomesCounts.DrawsCount}");
            counts.Append($"\n\n{" ",4} Total game rounds: {TotalRoundsCount}");
            counts.Append($"\n{" ",4}Total in-game time: {GameTime}");

            return $"##### {UserLogin} #####{history}{counts}\n";
        }
    }
}
