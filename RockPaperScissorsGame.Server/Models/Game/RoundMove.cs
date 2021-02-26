using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Server.Models.Game
{
    public class RoundMove
    {
        public readonly string PlayerId;
        public MoveOptions? SelectedOption { get; set; }

        public RoundMove(string playerId)
        {
            PlayerId = playerId;
        }
    }
}