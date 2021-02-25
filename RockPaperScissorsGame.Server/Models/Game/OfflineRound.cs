using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Server.Models.Game
{
    public class OfflineRound : RoundBase
    {
        public readonly MoveOptions BotMoveOption;
        public readonly MoveOptions UserMoveOption;
        public readonly GameOutcome RoundResult;
        
        public OfflineRound(string player1Id, MoveOptions userMove, MoveOptions botMove, GameOutcome result) : base(player1Id)
        {
            UserMoveOption = userMove;
            BotMoveOption = botMove;
            RoundResult = result;
        }
        
    }
}