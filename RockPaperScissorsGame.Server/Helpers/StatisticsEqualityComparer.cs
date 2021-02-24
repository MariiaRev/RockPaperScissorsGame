using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Helpers
{
    public class StatisticsEqualityComparer : IEqualityComparer<UserStatistics>
    {
        public bool Equals([AllowNull] UserStatistics statistics1, [AllowNull] UserStatistics statistics2)
        {
            //Check whether the compared objects reference the same data
            if (ReferenceEquals(statistics1, statistics2))
                return true;

            //Check whether any of the compared objects is null
            if (statistics1 is null || statistics2 is null)
                return false;

            //Check whether the objects' properties are equal
            return statistics1.UserId == statistics2.UserId;
        }

        public int GetHashCode([DisallowNull] UserStatistics statistics)
        {
            return statistics.UserId.GetHashCode();
        }
    }
}
