using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IBotGameService
    {
        MoveOptions MakeRandomChoice();
    }
}