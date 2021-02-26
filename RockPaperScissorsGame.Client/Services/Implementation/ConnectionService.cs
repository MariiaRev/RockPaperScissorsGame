using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class ConnectionService : IConnectionService
    {
        public HubConnection Connection { get; private set; }
        private readonly IOptions<HttpClientSettings> _clientSettings;

        public ConnectionService(IOptions<HttpClientSettings> clientSettings)
        {
            _clientSettings = clientSettings;
        }
        
        public async Task<bool> EnsureConnectionAsync(string playerId)
        {
            if (Connection == null || Connection.State != HubConnectionState.Connected)
            {
                await ConnectToTheServerAsync(playerId);
            }

            if (Connection != null && Connection.State == HubConnectionState.Connected)
            {
                return true;
            }
            
            throw new ConnectionException("\nUnable to connect to the server. Try again later");
        }
        
        private async Task ConnectToTheServerAsync(string playerId)
        {
            string signalRAddress = $"{_clientSettings.Value.BaseAddress}/GameHub?AuthToken={playerId}";

            Connection = new HubConnectionBuilder()
                .WithUrl(signalRAddress)
                .Build();

            Connection.On<string>(GameEvents.ReceiveMessage, Console.WriteLine);

            await ConnectAsync(Connection);
        }

        private async Task ConnectAsync(HubConnection connection)
        {
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    await connection.StartAsync();
                    return;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(500);
                }
            }
            
        }
    }
}