namespace RockPaperScissorsGame.Server.Models.Game
{
    public class GameRoom
    {
        public readonly string RoomToken;
        public readonly bool IsPrivate;
        public readonly Series Series;

        public GameRoom(string roomToken, string creatorId, bool isPrivate)
        {
            RoomToken = roomToken;
            IsPrivate = isPrivate;
            Series = new Series(creatorId);
        }
    }
}