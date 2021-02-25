namespace RockPaperScissorsGame.Events
{
    public class GameEvents
    {
        public const string ErrorOccured = "ErrorOccured";

        public const string ReceiveMessage = "ReceiveMessage";
        
        //Player has created a game and must wait for opponent to join
        public const string WaitingForPlayerToJoin = "WaitingForPlayerToJoin";
 
        //Both players have joined game
        public const string GameStart = "GameStart";
 
        //Player has chosen rock,paper or scissors but the opponent has not. Player must wait for opponent to make a choice
        public const string WaitingForPlayerToPlay = "WaitingForPlayerToPlay";
 
        //Round ended (may be canceled, also)
        public const string GameEnd = "GameEnd";

        //One player reached move timeout | round canceled
        public const string GameAborted = "GameAborted";

        //Game deleted
        public const string GameClosed = "GameClosed";
    }
}