using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RockPaperScissorsGame.Client.Models;
using RockPaperScissorsGame.Client.Options;

namespace RockPaperScissorsGame.Client.Services
{
    public class UserInteractions
    {
        private readonly IUserInput _userInput;
        private readonly UserInfoOptions _options;
        private readonly ForAuthorizationAndRegistration _authRegistration;
        private readonly RequestsForStatistics _requestsForStatistics;
        private readonly ISingleStorage<AuthToken> _authToken;

        private int _authorizationAttempts = 0;
        private DateTime? _authBlockedAt = null;

        public UserInteractions(
            IUserInput userInput,
            IOptions<UserInfoOptions> options,
            ForAuthorizationAndRegistration authRegistration,
            RequestsForStatistics requestsForStatistics,
            ISingleStorage<AuthToken> authToken)
        {
            _userInput = userInput;
            _options = options.Value;
            _authRegistration = authRegistration;
            _requestsForStatistics = requestsForStatistics;
            _authToken = authToken;
        }

        /// <summary>
        /// Interactions with user for authorization.
        /// Blocks authorization after <see cref="UserInfoOptions.AuthorizationAttemptsMax"/> unsuccessfull 
        /// authorization attempts for <see cref="UserInfoOptions.AuthorizationBlockingTime"/> seconds.
        /// </summary>
        /// <returns>
        /// True if user was authorized.
        /// False if user was not authorized.
        /// </returns>
        public async Task<bool> AuthorizeUserAsync()
        {    
            // check if authorization is blocked
            if (IsBlocked())
            {
                Console.WriteLine($"\n\nAuthorization is still blocked. Please, try again later.");
                return false;
            }

            // if authorization is not blocked

            // accept login and password
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    _authorizationAttempts++;
                    var authToken = await _authRegistration.AuthorizeAsync(login, password);

                    // if user was not authorized
                    if (authToken == null)
                    {
                        Console.WriteLine("\n\nWrong login or password.");
                        
                        if (_authorizationAttempts >= _options.AuthorizationAttemptsMax)
                        {
                            //block authorization for a while
                            Console.WriteLine($"\n\nYou used maximum ({_options.AuthorizationAttemptsMax}) authorization attempts.");
                            Console.WriteLine($"Authorization is blocked for {_options.AuthorizationBlockingTime} seconds.");
                            _authBlockedAt = DateTime.Now;
                            return false;
                        }
                        else
                        {
                            // try again or exit
                            var exitWord = "exit";
                            var message = $"Enter anything to retry authorization or enter '{exitWord}' to exit to the previous menu:";
                            
                            // if user entered anything but not the exit-word
                            if (!_userInput.ReadString(message, exitWord, true))
                            {
                                await AuthorizeUserAsync();
                            }

                            // if user entered the exit-word
                            return false;
                        }
                    }

                    // if user was authorized update his/her auth token
                    await _authToken.UpdateAsync(new AuthToken(authToken));
                    Console.WriteLine("\nYou are succesfully authorized.");
                    return true;
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return false;
        }

        /// <summary>
        /// Interactions with user for registration.
        /// </summary>
        /// <returns>Returns no value.</returns>
        public async Task RegisterUserAsync()
        {
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    (var success, var message ) = await _authRegistration.RegisterAsync(login, password);
                    Console.WriteLine($"\n\n{message}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public async Task ShowLeaderboardAsync()
        {
            // ask to send request
            (var success, var content) = await _requestsForStatistics.GetLeaderboardAsync();

            if (success)
            {
                try
                {
                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<List<UserStatistics>>(content);

                    // show leaderboard
                    Console.WriteLine($"\n\n{" ", 4}$$$ LEADERBOARD $$$\n");

                    foreach(var userStatistics in statistics)
                    {
                        Console.WriteLine($"{userStatistics}");
                    }

                    Console.WriteLine("-----------------------------------------------------------------");
                    return;
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    //logger log serialization error
                    return;
                }
            }

            // if no statistics
            Console.WriteLine($"\n\n{content}");
        }

        public async Task ShowUserStatisticsAsync()
        {
            try
            {
                // ask to send request
                (var success, var content) = await _requestsForStatistics.GetUserStatisticsAsync();

                if (success)
                {
                    try
                    {
                        // try deserialise json string (content) into list of UserStatistics
                        var statistics = JsonConvert.DeserializeObject<UserStatistics>(content);
                        Console.WriteLine($"\n\n{" ", 4}$$$ Your statistics $$$\n\n{statistics}");
                        Console.WriteLine("-----------------------------------------------------------------");
                        return;
                    }
                    catch (JsonSerializationException)
                    {
                        Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                        //logger log serialization error
                        return;
                    }
                }

                // if no statistics
                Console.WriteLine($"\n\n{content}");
            }
            catch(ArgumentNullException)
            {
                Console.WriteLine("\n\nPlease, authorize before viewing personal statistics.");
            }
        }

        private (string, string) AcceptLoginPassword()
        {
            var exitWord = "exit";
            var message = "\n\nEnter your login, please:";
            var tryAgainMessage = $"\n\nLogin should contain at least {_options.LoginMinLength} character(s). Try again or enter {exitWord} to exit:";
            var login = _userInput.ReadString(message, tryAgainMessage, _options.LoginMinLength, exitWord);

            if (login == null)
            {
                Console.WriteLine("\n\nAuthorization is canceled.");
                return (null, null);
            }

            message = "\n\nEnter your password, please:";
            tryAgainMessage = $"\n\nPassword should contain at least {_options.PasswordMinLength} character(s). Try again or enter {exitWord} to exit:";
            var password = _userInput.ReadString(message, tryAgainMessage, _options.PasswordMinLength, exitWord);

            if (password == null)
            {
                Console.WriteLine("\n\nAuthorization is canceled.");
                return (null, null);
            }

            return (login, password);
        }

        private bool IsBlocked()
        {
            if (_authBlockedAt == null)
            {
                return false;   // not blocked
            }

            if (DateTime.Now.Subtract((DateTime)_authBlockedAt).TotalSeconds > _options.AuthorizationBlockingTime)
            {
                // reset blocking settings
                _authorizationAttempts = 0;
                _authBlockedAt = null;

                return false;   // not blocked
            }

            return true;   // blocked
        }
    }
}
