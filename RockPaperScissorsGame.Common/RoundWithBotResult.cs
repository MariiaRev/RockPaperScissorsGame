using System.Text.Json.Serialization;

namespace RockPaperScissorsGame.Common
{
    public class RoundWithBotResult
    { 
       // [JsonPropertyName("botMoveOption")]
        public MoveOptions BotMoveOption { get; set; }
        
       // [JsonPropertyName("userMoveOption")]
        public MoveOptions UserMoveOption  { get; set; }
        
        // [JsonPropertyName("roundResult")]
        public string RoundResult  { get; set; }
    }
}