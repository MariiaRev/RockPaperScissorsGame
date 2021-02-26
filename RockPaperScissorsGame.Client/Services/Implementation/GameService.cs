using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class GameService : IGameService
    {
        private bool _isConnectionConfigured;
        private readonly IConnectionService _connectionService;
        private readonly HttpClient _client;
        private readonly ILogger<GameService> _logger;

        public GameService(IConnectionService connectionService, HttpClient client, IOptions<HttpClientSettings> options, ILogger<GameService> logger)
        {
            _connectionService = connectionService;
            _client = client;
            _logger = logger;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
        }

        private async Task<bool> EnsureConnectionConfigurationAsync(string playerId)
        {
            if (!await _connectionService.EnsureConnectionAsync(playerId)) return false;

            if (_isConnectionConfigured) return true;
            
            _isConnectionConfigured = true;
            return true;
        }
        
        public async Task<string> CreatePrivateRoomAsync(string playerId)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("CreatePrivateRoom");
                return resultEvent;
            }
            return GameEvents.ErrorOccured;
        }
        
        public async Task<string> JoinPrivateRoom(string playerId, string roomToken)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("JoinPrivateRoom", roomToken);
                return resultEvent;
            }
            
            return GameEvents.ErrorOccured;
        }

        public async Task<string> FindPublicGame(string playerId)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("FindPublicRoom");
                return resultEvent;
            }

            return GameEvents.ErrorOccured;
        }

        public async Task<string> PlayRoundWithBot(string playerId, MoveOptions moveOption)
        {
            var requestMessage = GetRequestMessage(
                HttpMethod.Post, 
                "bot/play",
                playerId,
                new StringContent(JsonSerializer.Serialize(moveOption.ToString()), Encoding.UTF8, "application/json"));

            try
            {
                var response = await _client.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var roundResult = JsonSerializer.Deserialize<RoundWithBotResult>(responseMessage);
                        if (roundResult == null)
                        {
                            _logger.LogError($"{nameof(GameService)}: Server returned unexpected response");
                            throw new ServiceException("Error occured. Try again later");
                        }
                        
                        return "\nRound summary\n" +
                                          $"Your choice: {roundResult.UserMoveOption}\n" +
                                          $"Bot's choice: {roundResult.BotMoveOption}\n" +
                                          $"Round result: {roundResult.RoundResult}\n";

                    }
                    catch (Exception exception)
                    {
                        if (exception is ArgumentNullException ||
                            exception is JsonException ||
                            exception is NotSupportedException)
                        {

                            _logger.LogError($"{nameof(GameService)}: Server returned unexpected response");
                            throw new ServiceException("Error occured. Try again later");
                        }

                        throw;
                    }
                }

                return "Error occured. Try again later";
            }
            catch (HttpRequestException)
            {
                throw new ConnectionException("Unable to connect to the server. Please, try again later");
            }
        }
        
        private HttpRequestMessage GetRequestMessage(HttpMethod method, string uri, string authToken, HttpContent content)
        {
            return new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(_client.BaseAddress + uri),
                Headers =
                {
                    { "X-AuthToken", authToken }
                },
                Content = content
            };
        }
    }
}