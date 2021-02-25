using System;
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
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Services;

namespace RockPaperScissorsGame.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Settings/appsettings.json", false)
                    .Build();

                var serviceProvider = new ServiceCollection()
                    .AddSingleton<IGamePlatform, GamePlatform>()
                    .AddSingleton<IInGamePlatform, InGamePlatform>()
                    .AddSingleton<IConnectionService, ConnectionService>()
                    .AddSingleton<IGameService, GameService>()
                    .AddSingleton<IInGameService, InGameService>()
                    .AddSingleton<Tests>()
                    .AddSingleton<ForAuthorizationAndRegistration>()
                    .AddSingleton<UserInteractions>()
                    .AddSingleton<IUserInput, UserInput>()
                    .AddSingleton(typeof(ISingleStorage<>), typeof(SingleStorage<>))
                    .Configure<ClientOptions>(configuration.GetSection("ClientSettings"))
                    .Configure<UserInfoOptions>(configuration.GetSection("UserInfoSettings"))
                    .Configure<AppSettings>(configuration.GetSection("App"))
                    .AddHttpClient()
                    .AddLogging(builder => builder.AddSerilog(
                        new LoggerConfiguration()
                            .WriteTo.File("Logs/client.log")
                            .CreateLogger()))
                    .BuildServiceProvider();

                //await serviceProvider.GetRequiredService<Tests>().RunAsync();
                // var gamePlatform = serviceProvider.GetRequiredService<IGamePlatform>();
                //await gamePlatform.StartAsync("X-PLAYER_ID");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message}\n\n\n");
            }
        }
    }
}
