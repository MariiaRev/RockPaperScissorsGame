using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Implementation;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Services.Implementation;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Settings\\appsettings.json", optional: false)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IGamePlatform, GamePlatform>()
                .AddSingleton<IInGamePlatform, InGamePlatform>()
                .AddSingleton<IConnectionService, ConnectionService>()
                .AddSingleton<IGameService, GameService>()
                .AddSingleton<IInGameService, InGameService>()
                .Configure<AppSettings>(configuration.GetSection("App"))
                .AddLogging(builder => builder.AddSerilog(
                    new LoggerConfiguration()
                        .WriteTo.File("Logs/client.log")
                        .CreateLogger()))
                .BuildServiceProvider();

            var gamePlatform = serviceProvider.GetRequiredService<IGamePlatform>();
            await gamePlatform.StartAsync("X-PLAYER_ID");
        }
    }
}
