using System.Threading.Tasks;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Services
{
    public interface IStatisticsService
    {
        Task<bool> SaveAsync(string token, GameOutcome outcome, MoveOptions move);
        void SetupStorage();
    }
}
