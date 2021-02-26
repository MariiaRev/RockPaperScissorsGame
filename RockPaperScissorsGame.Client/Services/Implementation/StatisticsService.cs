using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Models;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class StatisticsService : IStatisticsService
    {
        private readonly HttpClient _client;
        private readonly ISingleStorage<AuthInfo> _authInfo;
        private readonly ILogger<StatisticsService> _logger;
        
        public StatisticsService(
            HttpClient client,
            IOptions<ClientSettings> options,
            ISingleStorage<AuthInfo> authInfo,
            ILogger<StatisticsService> logger)
        {
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
            _authInfo = authInfo;
            _logger = logger;
        }
        
        /// <summary>
        /// Request to the server for the general statistics (leaderboard).
        /// </summary>
        /// <returns>
        /// Cortege (true, content) - if general statistics is not empty and content contains statistics by each user.
        /// Cortege (false, content) - if statistics is empty and content contains some message from the server.
        /// </returns>
        public async Task<(bool, string)> GetLeaderboardAsync()
        {
            _logger.LogInformation($"{nameof(StatisticsService)}: Sending request to get the leaderboard.");
            try
            {
                var response = await _client.GetAsync("statistics");
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation($"{nameof(StatisticsService)}: The leaderboard is received.");
                    return (true, content);
                }

                _logger.LogInformation($"{nameof(StatisticsService)}: {content}");
                return (false, content);
            }
            catch (HttpRequestException)
            {
                _logger.LogError($"{nameof(StatisticsService)}: Unable to connect to the server.");
                throw new ConnectionException("Unable to connect to the server. Please, try again later");
            }
        }
        
        /// <summary>
        /// Request to the server for the user personal statistics.
        /// </summary>
        /// <returns>
        /// Cortege (true, content) - if user statistics exists and content contains user statistics (empty or not).
        /// Cortege (false, content) - if statistics does not exist and content contains some message from the server.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when unauthorized user tries to get personal statistics.</exception>
        public async Task<(bool, string)> GetUserStatisticsAsync()
        {
            var authInfo = _authInfo.Get();
            if (authInfo == null || authInfo?.Token == null)
            {
                var message = "Unauthorized user cannot get personal statistics.";
                _logger.LogInformation($"{nameof(StatisticsService)}: {message}");
                throw new ArgumentNullException(nameof(_authInfo), message);
            }

            _logger.LogInformation($"{nameof(StatisticsService)}: Sending request to get user's personal statistics.");
            var requestMessage = GetRequestMessage(HttpMethod.Get, "statistics/user", new StringContent(string.Empty));
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"{nameof(StatisticsService)}: User's personal statistics is received.");
                return (true, content);
            }

            _logger.LogInformation($"{nameof(StatisticsService)}: {content}.");
            return (false, content);
        }
        
        public async Task<bool> SaveUserGameTime()
        {
            var authInfo = _authInfo.Get();

            if (authInfo == null || authInfo?.Token == null)
            {
                var message = "Unauthorized user cannot save his/her in-game time.";
                _logger.LogInformation($"{nameof(StatisticsService)}: {message}");
                throw new ArgumentNullException(nameof(_authInfo), message);
            }

            // get in-game time for now
            var userGameTime = authInfo.Watch.ElapsedTicks.ToString();
            _logger.LogInformation($"{nameof(StatisticsService)}: Calculated user in-game time. Sending request to save the time.");

            var requestMessage = GetRequestMessage(HttpMethod.Post, "gametime", new StringContent(string.Empty), userGameTime);
            var response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // in-game time was saved, then, restart the watch
                authInfo.Watch.Restart();
                _logger.LogInformation($"{nameof(StatisticsService)}: The in-game time was successfully saved.");
                return true;
            }

            _logger.LogInformation($"{nameof(StatisticsService)}: The in-game time was not saved.");
            return false;
        }
        
        private HttpRequestMessage GetRequestMessage(HttpMethod method, string uri, HttpContent content, string gameTime = null)
        {
            if (gameTime == null)
            {
                return new HttpRequestMessage()
                {
                    Method = method,
                    RequestUri = new Uri(_client.BaseAddress + uri),
                    Headers =
                    {
                        { "X-AuthToken", _authInfo.Get().Token }
                    },
                    Content = content
                };
            }

            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(_client.BaseAddress + uri),
                Headers =
                {
                    { "X-AuthToken", _authInfo.Get().Token },
                    { "X-Time", gameTime }
                },
                Content = content
            };
        }
    }
}