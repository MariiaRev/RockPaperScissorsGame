using System;
using System.Linq;
using System.Collections.Generic;

namespace RockPaperScissorsGame.Server.Models
{
    public class UserStatistics
    {
        public int UserId { get; private set; }
        public Dictionary<string, GameOutcomeCounts> History { get; private set; }
        public uint RockCount { get; private set; }
        public uint PaperCount { get; private set; }
        public uint ScissorsCount { get; private set; }
        public long GameTime { get; private set; }

        public UserStatistics(int id)
        {
            UserId = id;
            History = new Dictionary<string, GameOutcomeCounts>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="history">History of game outcomes by day</param>
        /// <param name="rockCount">Total count of used move <see cref="MoveOptions.Rock"/></param>
        /// <param name="paperCount">Total count of used move <see cref="MoveOptions.Paper"/></param>
        /// <param name="scissorsCount">Total count of used move <see cref="MoveOptions.Scissors"/></param>
        /// <param name="gameTime">Total game time in ticks</param>
        public UserStatistics(
            int id,
            Dictionary<string, GameOutcomeCounts> history, 
            uint rockCount, uint paperCount, uint scissorsCount,
            long gameTime)
        {
            UserId = id;
            History = history;
            RockCount = rockCount;
            PaperCount = paperCount;
            ScissorsCount = scissorsCount;
            GameTime = gameTime;
        }

        /// <summary>
        /// Saves information and results of the played game round.
        /// </summary>
        /// <param name="date">The date of the played round.</param>
        /// <param name="outcome">The round outcome. One of <see cref="GameOutcome"/></param>
        /// <param name="move">Player's move. One of <see cref="MoveOptions"/></param>
        public void AddRoundInfo(DateTime date, GameOutcome outcome, MoveOptions move)
        {
            var stringDate = date.ToString("dd.MM.yyyy");
            History.TryAdd(stringDate, new GameOutcomeCounts());
            History[stringDate].Increment(outcome);
            IncrementMoveOptionCount(move);
        }

        /// <summary>
        /// Adds the player's in-game time to the previous one.
        /// </summary>
        /// <param name="gameTime">Player's in-game time in ticks</param>
        public void AddGameTime(long gameTime)
        {
            GameTime += gameTime;
        }

        /// <summary>
        /// Converts the game time to a human-readable string.
        /// </summary>
        /// <returns>In-game time in a human-readable string.</returns>
        public string GameTimeToString()
        {
            var time = new TimeSpan(GameTime);
            return $"{time.Days} day(s) {time.Hours} hour(s) {time.Minutes} minute(s)";
        }

        /// <summary>
        /// Get total number of the player's rounds.
        /// </summary>
        /// <returns>Total number of played rounds.</returns>
        public uint GetRoundsCount()
        {
            return RockCount + PaperCount + ScissorsCount;
        }

        /// <summary>
        /// Get the total of each outcome.
        /// </summary>
        /// <returns><see cref="GameOutcomeCounts"/> with total count for each outcome.</returns>
        public GameOutcomeCounts GetTotalOutcomeCounts()
        {
            var winsCount = (uint)History.Sum(x => x.Value.WinsCount);
            var lossesCount = (uint)History.Sum(x => x.Value.LossesCount);
            var drawsCount = (uint)History.Sum(x => x.Value.DrawsCount);

            return new GameOutcomeCounts()
            {
                WinsCount = winsCount,
                LossesCount = lossesCount,
                DrawsCount = drawsCount
            };
        }

        private void IncrementMoveOptionCount(MoveOptions move)
        {
            switch (move)
            {
                case MoveOptions.Rock: RockCount++; break;
                case MoveOptions.Paper: PaperCount++; break;
                case MoveOptions.Scissors: ScissorsCount++; break;
                default: throw new ArgumentOutOfRangeException(nameof(move), "Undefined move option.");
            }
        }
    }
}
