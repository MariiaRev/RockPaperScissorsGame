using System.Diagnostics;

namespace RockPaperScissorsGame.Client.Models
{
    public class AuthInfo
    {
        public string Token { get; }
        public Stopwatch Watch { get; }
        public AuthInfo(string token)
        {
            Token = token;
            Watch = new Stopwatch();
            Watch.Start();
        }
    }
}
