using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class SigningService : ISigningService
    {
        private readonly HttpClient _client;

        public SigningService(HttpClient client, IOptions<ClientSettings> options)
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
        public async Task<string> AuthorizeAsync(string login, string password)
        {
            var requestMessage = GetRequestMessage(
                HttpMethod.Post, "login", 
                login, password, 
                new StringContent(string.Empty));

            try
            {
                var response = await _client.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    return token;
                }

                return null;
            }
            catch (HttpRequestException exception)
            {
                throw new ConnectionException("Unable to connect to the server. Please, try again later");
            }
            
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
        public async Task<(bool, string)> RegisterAsync(string login, string password)
        {
            var requestMessage = GetRequestMessage(
                HttpMethod.Post, "registration", 
                login, password, 
                new StringContent(string.Empty));

            try
            {
                var response = await _client.SendAsync(requestMessage);
                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return (true, responseMessage);
                }

                return (false, responseMessage);
            }
            catch (HttpRequestException exception)
            {
                throw new ConnectionException("Unable to connect to the server. Please, try again later");
            }
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