using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Models;
using System.Net;

namespace RockPaperScissorsGame.Client.Services
{
    public class RequestsForStatistics
    {
        private readonly HttpClient _client;
        private readonly ISingleStorage<AuthToken> _authToken;
        public RequestsForStatistics(
            HttpClient client,
            IOptions<ClientOptions> options,
            ISingleStorage<AuthToken> authToken)
        {
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
            _authToken = authToken;
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
            var response = await _client.GetAsync("statistics");
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return (true, content);
            }

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
            if (_authToken.Get() == null || _authToken.Get()?.Token == null)
            {
                throw new ArgumentNullException(nameof(_authToken), "Unauthorized user cannot get personal statistics.");
            }

            var requestMessage = GetRequestMessage(HttpMethod.Get, "statistics/user", new StringContent(string.Empty));
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return (true, content);
            }

            return (false, content);
        }

        private HttpRequestMessage GetRequestMessage(HttpMethod method, string uri, HttpContent content)
        {
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(_client.BaseAddress + uri),
                Headers =
                {
                    { "X-AuthToken", _authToken.Get().Token }
                },
                Content = content
            };
        }
    }
}
