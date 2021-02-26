using System.Threading.Tasks;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Services.Abstract
{
    public interface IInGameService
    {
        Task MakeMoveAsync(string playerId, MoveOptions figure, bool isMoveMadeInTime);
        Task LeaveGameAsync(string playerId);
    }
}