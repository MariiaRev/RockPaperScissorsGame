using System;

namespace RockPaperScissorsGame.Server.Models
{
    public class GameOutcomeCounts
    {
        public uint WinsCount { get; set; } = 0;
        public uint LossesCount { get; set; } = 0;
        public uint DrawsCount { get; set; } = 0;

        public void Increment(GameOutcome outcome)
        {
            switch(outcome)
            {
                case GameOutcome.Win: WinsCount++; break;
                case GameOutcome.Loss: LossesCount++; break;
                case GameOutcome.Draw: DrawsCount++; break;
                default: throw new ArgumentOutOfRangeException(nameof(outcome), "There cannot be another outcome.");
            }
        }
    }
}
