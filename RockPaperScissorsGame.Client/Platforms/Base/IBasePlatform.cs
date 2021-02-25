using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Platforms.Base
{
    public interface IBasePlatform
    {
        Task StartAsync(string playerId);
    }
}