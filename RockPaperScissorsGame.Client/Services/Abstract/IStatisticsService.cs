using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Services.Abstract
{
    public interface IStatisticsService
    {
        Task<(bool, string)> GetLeaderboardAsync();
        Task<(bool, string)> GetUserStatisticsAsync();
        Task<bool> SaveUserGameTime();

    }
}