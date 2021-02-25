using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Models
{
    public class RoundWithBotResult
    {
        public MoveOptions BotMoveOption { get; set; }
        public MoveOptions UserMoveOption { get; set; }
        public string RoundResult { get; set; }

        public RoundWithBotResult(MoveOptions botMoveOption, MoveOptions userMoveOption, string roundResult)
        {
            BotMoveOption = botMoveOption;
            UserMoveOption = userMoveOption;
            RoundResult = roundResult;
        }
    }
}