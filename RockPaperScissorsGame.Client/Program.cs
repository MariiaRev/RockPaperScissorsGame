using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Services;
using RockPaperScissorsGame.Client.Models;

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
                    .AddSingleton<Tests>()
                    .AddSingleton<ForAuthorizationAndRegistration>()
                    .AddSingleton<UserInteractions>()
                    .AddSingleton<IUserInput, UserInput>()
                    .AddSingleton(typeof(ISingleStorage<>), typeof(SingleStorage<>))
                    .Configure<ClientOptions>(configuration.GetSection("ClientSettings"))
                    .Configure<UserInfoOptions>(configuration.GetSection("UserInfoSettings"))
                    .AddHttpClient()
                    .BuildServiceProvider();

                await serviceProvider.GetRequiredService<Tests>().RunAsync();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message}\n\n\n");
            }

            // can I get authToken from here?
        }
    }
}
