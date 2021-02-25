using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using RockPaperScissorsGame.Client.Options;
using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client
{
    /// <summary>
    /// Methods from here send requests to the server, receive responses and return responses content.
    /// </summary>
    public class ForAuthorizationAndRegistration
    {
        private readonly HttpClient _client;
        public ForAuthorizationAndRegistration(HttpClient client, IOptions<ClientOptions> options)
        {
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
        }

        /// <summary>
        /// Send request to the server to authorize user with <paramref name="login"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="password">User password.</param>
        /// <returns>
        /// String authorization token if user was authorized.
        /// Null if user was not authorized (for example, because of wrong login or password).
        /// </returns>
        public async Task<string> Authorize(string login, string password)      //login request
        {
            var requestMessage = GetRequestMessage(
                HttpMethod.Post, "login", 
                login, password, 
                new StringContent(string.Empty));

            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }

            return null;
        }

        /// <summary>
        /// Send request to the server to register user with <paramref name="login"/> and <paramref name="password"/>.
        /// <param name="login">User login.</param>
        /// <param name="password">User password.</param>
        /// </summary>
        /// <param name="login">user login.</param>
        /// <param name="password">User password.</param>
        /// <returns>
        ///Returns true with success message if user was registered.
        ///Returns false with error message if user was not registered (for example, because <paramref name="login"/> already exists).
        /// </returns>
        public async Task<(bool, string)> Register(string login, string password)   // registration request
        {
            var requestMessage = GetRequestMessage(
                HttpMethod.Post, "registration", 
                login, password, 
                new StringContent(string.Empty));

            var response = await _client.SendAsync(requestMessage);
            var responseMessage = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return (true, responseMessage);
            }

            return (false, responseMessage);
        }

        private HttpRequestMessage GetRequestMessage(HttpMethod method, string uri, string login, string password, HttpContent content)
        {
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(_client.BaseAddress + uri),
                Headers =
                {
                    { "X-login", login },
                    { "X-password", password }
                },
                Content = content
            };
        }
    }
}
