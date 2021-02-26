namespace RockPaperScissorsGame.Server.Models.Game
{
    public abstract class RoundBase
    {
        public readonly RoundMove Player1;

        protected RoundBase(string player1Id)
        {
            Player1 = new RoundMove(player1Id);
        }
    }
}