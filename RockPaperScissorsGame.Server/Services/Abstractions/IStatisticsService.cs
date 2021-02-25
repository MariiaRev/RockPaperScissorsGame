using System.Threading.Tasks;
using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Game;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IStatisticsService
    {
        Task<bool> SaveAsync(string token, GameOutcome outcome, MoveOptions move);
        void SetupStorage();
    }
}
