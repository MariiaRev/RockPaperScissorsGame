using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Server.Models.Out
{
    public class RoundWithBotResultOut
    { 
        public readonly MoveOptions BotMoveOption;
        public readonly MoveOptions UserMoveOption;
        public readonly string RoundResult;

        public RoundWithBotResultOut(MoveOptions botMoveOption, MoveOptions userMoveOption, string roundResult)
        {
            BotMoveOption = botMoveOption;
            UserMoveOption = userMoveOption;
            RoundResult = roundResult;
        }
    }
}