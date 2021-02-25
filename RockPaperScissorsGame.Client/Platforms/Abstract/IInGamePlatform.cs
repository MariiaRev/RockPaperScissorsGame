using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Platforms.Abstract
{
    public interface IInGamePlatform
    {
        public Task StartAsync(int waitTimeSecs);
    }
}