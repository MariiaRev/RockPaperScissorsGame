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
using RockPaperScissorsGame.Client;

namespace RockPaperScissorsGame.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Settings/appsettings.json", false)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<Tests>()
                .AddSingleton<ForAuthorizationAndRegistration>()
                .Configure<ClientOptions>(configuration.GetSection("ClientSettings"))
                .AddHttpClient()
                .BuildServiceProvider();

            await serviceProvider.GetRequiredService<Tests>().RunAsync();
        }
    }
}
