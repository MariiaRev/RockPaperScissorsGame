namespace RockPaperScissorsGame.Server.Models.Game
{
    public class OnlineRound : RoundBase
    {
        public readonly RoundMove Player2;

        public OnlineRound(string player1Id, string player2Id) : base(player1Id)
        {
            Player2 = new RoundMove(player2Id);
        }
        
        
    }
}
