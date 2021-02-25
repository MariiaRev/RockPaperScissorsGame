using RockPaperScissorsGame.Server.Models.Game;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IGameStoringService
    {
        string GenerateRoomToken();
        bool CreateRoom(string token, bool isPrivate, string player1Id);
        string FindPublicRoom();
        bool JoinRoom(string roomToken, string playerId);
        GameRoom GetRoomByPlayer(string playerId);
        bool IsPrivateRoomExists(string roomToken);
        bool DeleteRoom(string roomToken);
    }
}
