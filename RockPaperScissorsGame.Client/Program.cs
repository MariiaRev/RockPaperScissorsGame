using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Helpers.Implementations;
using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Implementation;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Services.Implementation;
using RockPaperScissorsGame.Client.Settings;
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
                    .AddSingleton<IMainPlatform, MainPlatform>()
                    .AddSingleton<IGamePlatform, GamePlatform>()
                    .AddSingleton<IInGamePlatform, InGamePlatform>()
                    .AddSingleton<IConnectionService, ConnectionService>()
                    .AddSingleton<IGameService, GameService>()
                    .AddSingleton<IInGameService, InGameService>()
                    .AddSingleton<ISigningService, SigningService>()
                    .AddSingleton<IUserInput, UserInput>()
                    .AddSingleton(typeof(ISingleStorage<>), typeof(SingleStorage<>))
                    //.AddSingleton<ForAuthorizationAndRegistration>()
                    //.AddSingleton<UserInteractions>()
                    .AddSingleton<RequestsForStatistics>()

                    .Configure<ClientSettings>(configuration.GetSection("ClientSettings"))
                    .Configure<UserInfoSettings>(configuration.GetSection("UserInfoSettings"))
                    .Configure<TimeoutSettings>(configuration.GetSection("App"))
                    
                    .AddHttpClient()
                    
                    .AddLogging(builder => builder.AddSerilog(
                        new LoggerConfiguration()
                            .WriteTo.File("Logs/app.log")
                            .CreateLogger(), true))
                    
                    .BuildServiceProvider();

                var mainPlatform = serviceProvider.GetRequiredService<IMainPlatform>();
                await mainPlatform.StartAsync(null);
                
                //await serviceProvider.GetRequiredService<Tests>().RunAsync();
                // var gamePlatform = serviceProvider.GetRequiredService<IGamePlatform>();
                //await gamePlatform.StartAsync("X-PLAYER_ID");
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine($"{exception.Message}\n\n");
            }
        }
    }
}
