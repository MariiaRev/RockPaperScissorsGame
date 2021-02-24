using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Helpers
{
    public class UserEqualityComparer : IEqualityComparer<User>
    {
        public bool Equals([AllowNull] User user1, [AllowNull] User user2)
        {
            //Check whether the compared objects reference the same data
            if (ReferenceEquals(user1, user2))
                return true;

            //Check whether any of the compared objects is null
            if (user1 is null ||user2 is null)
                return false;

            //Check whether the objects' properties are equal
            return user1.GetLogin() == user2.GetLogin();
        }

        public int GetHashCode([DisallowNull] User user)
        {
            return user.GetLogin().GetHashCode();
        }
    }
}
