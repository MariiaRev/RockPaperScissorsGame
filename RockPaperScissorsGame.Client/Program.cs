using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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

                var serilogLogger = new LoggerConfiguration()
                .WriteTo.File("Logs/app.log")
                .CreateLogger();

                var serviceProvider = ConfigureServices(new ServiceCollection(), configuration, serilogLogger).BuildServiceProvider();
                    
                await serviceProvider.GetRequiredService<Tests>().RunAsync();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message}\n\n\n");
            }
        }
        private static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            return services.AddSingleton<Tests>()
                .AddSingleton<ForAuthorizationAndRegistration>()
                .AddSingleton<RequestsForStatistics>()
                .AddSingleton<UserInteractions>()
                .AddSingleton<IUserInput, UserInput>()
                .AddSingleton(typeof(ISingleStorage<>), typeof(SingleStorage<>))
                .Configure<ClientOptions>(configuration.GetSection("ClientSettings"))
                .Configure<UserInfoOptions>(configuration.GetSection("UserInfoSettings"))
                .AddHttpClient()
                .AddLogging(builder =>
                {
                    builder.AddSerilog(logger, true);
                });
        }
    }
}
