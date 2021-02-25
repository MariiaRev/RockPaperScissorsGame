using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Services.Abstract
{
    public interface IGameService
    {
        Task<string> CreatePrivateRoomAsync(string playerId);
        Task<string> JoinPrivateRoom(string playerId, string roomToken);
        Task<string> FindPublicGame(string playerId);
    }
}