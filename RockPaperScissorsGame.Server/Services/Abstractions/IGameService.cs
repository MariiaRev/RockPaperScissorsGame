using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Game;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IGameService
    {
        GameOutcome GetGameResult(MoveOptions playerChoice, MoveOptions opponentChoice);
    }
}
