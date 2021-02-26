using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Models;

namespace RockPaperScissorsGame.Client.Services
{
    public class RequestsForStatistics
    {
        private readonly HttpClient _client;
        private readonly ISingleStorage<AuthInfo> _authInfo;
        private readonly ILogger<RequestsForStatistics> _logger;
        public RequestsForStatistics(
            HttpClient client,
            IOptions<ClientOptions> options,
            ISingleStorage<AuthInfo> authInfo,
            ILogger<RequestsForStatistics> logger)
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
            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: Sending request to get the leaderboard.");
            var response = await _client.GetAsync("statistics");
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: The leaderboard is received.");
                return (true, content);
            }

            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: {content}");
            return (false, content);
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
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: {message}");
                throw new ArgumentNullException(nameof(_authInfo), message);
            }

            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: Sending request to get user's personal statistics.");
            var requestMessage = GetRequestMessage(HttpMethod.Get, "statistics/user", new StringContent(string.Empty));
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: User's personal statistics is received.");
                return (true, content);
            }

            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: {content}.");
            return (false, content);
        }

        public async Task<bool> SaveUserGameTime()
        {
            var authInfo = _authInfo.Get();

            if (authInfo == null || authInfo?.Token == null)
            {
                var message = "Unauthorized user cannot save his/her in-game time.";
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: {message}");
                throw new ArgumentNullException(nameof(_authInfo), message);
            }

            // get in-game time for now
            var userGameTime = authInfo.Watch.ElapsedTicks.ToString();
            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: Calculated user in-game time. Sending request to save the time.");

            var requestMessage = GetRequestMessage(HttpMethod.Post, "gametime", new StringContent(string.Empty), userGameTime);
            var response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // in-game time was saved, then, restart the watch
                authInfo.Watch.Restart();
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: The in-game time was successfully saved.");
                return true;
            }

            _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: The in-game time was not saved.");
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
