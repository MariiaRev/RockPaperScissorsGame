using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Db;
using RockPaperScissorsGame.Server.Models.Out;

namespace RockPaperScissorsGame.Server.Helpers
{
    /// <summary>
    /// Converts models from <see cref="RockPaperScissorsGame.Server.Models"/> to their other matching models.
    /// </summary>
    public static class ModelsMapper
    {
        /// <summary>
        /// Converts <see cref="UserStatistics"/> model  to the <see cref="StatisticsDb"/> one.
        /// </summary>
        /// <param name="statistics"><see cref="UserStatistics"/> statistics to convert.</param>
        /// <returns>Converted <see cref="StatisticsDb"/> statistics.</returns>
        public static StatisticsDb ToStatisticsDb(UserStatistics statistics)
        {
            return new StatisticsDb()
            {
                UserId = statistics.UserId,
                History = statistics.History,
                RockCount = statistics.RockCount,
                PaperCount = statistics.PaperCount,
                ScissorsCount = statistics.ScissorsCount,
                GameTime = statistics.GameTime
            };
        }

        /// <summary>
        /// Converts <see cref="StatisticsDb"/> model  to the <see cref="UserStatistics"/> one.
        /// </summary>
        /// <param name="statistics"><see cref="StatisticsDb"/> statistics to convert.</param>
        /// <returns>Converted <see cref="UserStatistics"/> statistics.</returns>
        public static UserStatistics ToUserStatistics(StatisticsDb statistics)
        {
            return new UserStatistics(
                statistics.UserId,
                statistics.History,
                statistics.RockCount,
                statistics.PaperCount,
                statistics.ScissorsCount,
                statistics.GameTime);
        }

        /// <summary>
        /// Converts <see cref="UserStatistics"/> model  to the <see cref="StatisticsOut"/> one.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="statistics"><see cref="UserStatistics"/> statistics to convert
        /// for the user with login <paramref name="userLogin"/>.</param>
        /// <returns>Converted <see cref="StatisticsOut"/> statistics.</returns>
        public static StatisticsOut ToStatisticsOut(string userLogin, UserStatistics statistics)
        {
            return new StatisticsOut()
            {
                UserLogin = userLogin,
                History = statistics.History,
                RockCount = statistics.RockCount,
                PaperCount = statistics.PaperCount,
                ScissorsCount = statistics.ScissorsCount,
                TotalRoundsCount = statistics.GetRoundsCount(),
                TotalOutcomesCounts = statistics.GetTotalOutcomeCounts(),
                GameTime = statistics.GameTimeToString()
            };
        }

        /// <summary>
        /// Converts <see cref="User"/> model to the <see cref="UserDb"/> one.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="user"><see cref="User"/> user to convert.</param>
        /// <returns>Converted <see cref="UserDb"/> user.</returns>
        public static UserDb ToUserDb(int userId, User user)
        {
            return new UserDb()
            {
                UserId = userId,
                Login = user.GetLogin(),
                Password = user.GetPassword()
            };
        }

        /// <summary>
        /// Converts <see cref="UserDb"/> model to the <see cref="User"/> one.
        /// </summary>
        /// <param name="user"><see cref="UserDb"/> user to convert.</param>
        /// <returns>Converted <see cref="User"/> user.</returns>
        public static User ToUser(UserDb user)
        {
            return new User(user.Login, user.Password);
        }
    }
}