using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RockPaperScissorsGame.Client.Models;
using RockPaperScissorsGame.Client.Helpers;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Helpers.Implementations;
using RockPaperScissorsGame.Client.Settings;


namespace RockPaperScissorsGame.Client.Services
{
    public class UserInteractions
    {
        private readonly IUserInput _userInput;
        private readonly UserInfoSettings _settings;
        private readonly ForAuthorizationAndRegistration _authRegistration;
        private readonly RequestsForStatistics _requestsForStatistics;
        private readonly ISingleStorage<AuthInfo> _authInfo;
        private readonly ILogger<UserInteractions> _logger;

        private int _authorizationAttempts = 0;
        private DateTime? _authBlockedAt = null;

        public UserInteractions(
            IUserInput userInput,
            IOptions<UserInfoSettings> settings,
            ForAuthorizationAndRegistration authRegistration,
            RequestsForStatistics requestsForStatistics,
            ISingleStorage<AuthInfo> authInfo,
            ILogger<UserInteractions> logger)
        {
            _userInput = userInput;
            _settings = settings.Value;
            _authRegistration = authRegistration;
            _requestsForStatistics = requestsForStatistics;
            _authInfo = authInfo;
            _logger = logger;
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
            _logger.LogInformation($"{nameof(UserInteractions)}: Attempt for user authorization.");

            // check if authorization is blocked
            if (IsBlocked())
            {
                _logger.LogInformation($"{nameof(UserInteractions)}: Authorization is blocked.");
                Console.WriteLine($"\nAuthorization is still blocked. Please, try again later.");
                return false;
            }

            // if authorization is not blocked
            _logger.LogInformation($"{nameof(UserInteractions)}: Authorization is not blocked.");

            // accept login and password
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    _authorizationAttempts++;
                    _logger.LogInformation($"{nameof(UserInteractions)}: The number of authorization attempts has been increased.");
                    var authToken = await _authRegistration.AuthorizeAsync(login, password);                    

                    // if user was not authorized
                    if (authToken == null)
                    {
                        _logger.LogInformation($"{nameof(UserInteractions)}: Wrong login or/and password.");
                        Console.WriteLine("\nWrong login or password.");
                        
                        if (_authorizationAttempts >= _settings.AuthorizationAttemptsMax)
                        {
                            //block authorization for a while
                            Console.WriteLine($"\n\nYou used maximum ({_settings.AuthorizationAttemptsMax}) authorization attempts.");
                            Console.WriteLine($"Authorization is blocked for {_settings.AuthorizationBlockingTime} seconds.");
                            _authBlockedAt = DateTime.Now;
                            _logger.LogInformation($"{nameof(UserInteractions)}: Authorization was blocked at {_authBlockedAt} for " +
                                $"{_settings.AuthorizationBlockingTime} seconds after {_authorizationAttempts} unsuccessful attempts.");
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
                                _logger.LogInformation($"{nameof(UserInteractions)}: User chose one more authorization attempt.");
                                await AuthorizeUserAsync();
                            }

                            // if user entered the exit-word
                            _logger.LogInformation($"{nameof(UserInteractions)}: The authorization is canceled by user.");
                            return false;
                        }
                    }

                    // if user was authorized update his/her auth token
                    await _authInfo.UpdateAsync(new AuthInfo(authToken));
                    _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: A new authorization token was assigned to the user.");

                    Console.WriteLine("\nYou are successfully authorized.");
                    _logger.LogInformation($"{nameof(UserInteractions)}: Successful authorization of the user.");
                    return true;
                }
                catch(Exception e)
                {
                    _logger.LogInformation($"{nameof(UserInteractions)}: {e}.");
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
            _logger.LogInformation($"{nameof(UserInteractions)}: Attempt for registration a new user.");
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    (var success, var message ) = await _authRegistration.RegisterAsync(login, password);
                    Console.WriteLine($"\n{message}");
                    _logger.LogInformation($"{nameof(UserInteractions)}: {message}.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public async Task ShowLeaderboardAsync()
        {
            _logger.LogInformation($"{nameof(UserInteractions)}: Attempt to view the leaderboard.");

            // if user is authorized save his/her in-game time
            var authInfo = _authInfo.Get();
            if (authInfo != null && authInfo?.Token != null)
            {
                // ask to send request to save user in-game time
                await _requestsForStatistics.SaveUserGameTime();
            }

            // ask to send request for statistics
            (var success, var content) = await _requestsForStatistics.GetLeaderboardAsync();

            if (success)
            {
                try
                {
                    _logger.LogInformation($"{nameof(UserInteractions)}: The leaderboard is recieved.");

                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<List<UserStatistics>>(content);
                    _logger.LogInformation($"{nameof(UserInteractions)}: The leaderboard is deserialised.");

                    if (statistics == null)
                    {
                        Console.WriteLine($"\n\nNo statistics in the leaderboard yet.");
                        return;
                    }

                    // show leaderboard
                    Console.WriteLine($"\n\n{" ", 4}$$$ LEADERBOARD $$$\n");

                    foreach(var userStatistics in statistics)
                    {
                        Console.WriteLine($"{userStatistics}");
                    }

                    Console.WriteLine("-----------------------------------------------------------------");
                    _logger.LogInformation($"{nameof(UserInteractions)}: Statistics for {statistics.Count} user(s) was shown.");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    _logger.LogInformation($"{nameof(UserInteractions)}: Exception during the leaderboard deserialization from json.");
                }

                return;
            }

            // if no statistics
            Console.WriteLine($"\n\n{content}");
            _logger.LogInformation($"{nameof(UserInteractions)}: {content}.");
        }

        public async Task ShowUserStatisticsAsync()
        {
            var authInfo = _authInfo.Get();
            if (authInfo == null || authInfo?.Token == null)
            {
                Console.WriteLine("\n\nPlease, authorize before viewing personal statistics.");
                _logger.LogInformation($"{nameof(UserInteractions)}: The user is unathorized and cannot view personal statistics.");
                return;
            }

            // if user is authorized
            // ask to send request to save user in-game time
            await _requestsForStatistics.SaveUserGameTime();

            // ask to send request for statistics
            (var success, var content) = await _requestsForStatistics.GetUserStatisticsAsync();

            if (success)
            {
                try
                {
                    _logger.LogInformation($"{nameof(UserInteractions)}: The user statistics is recieved.");

                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<UserStatistics>(content);
                    _logger.LogInformation($"{nameof(UserInteractions)}: The user statistics is deserialized and shown.");
                    Console.WriteLine($"\n\n{" ", 4}$$$ Your statistics $$$\n\n{statistics}");
                    Console.WriteLine("-----------------------------------------------------------------");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    _logger.LogInformation($"{nameof(UserInteractions)}: Exception during the user statistics deserialization from json.");
                }

                return;
            }

            // if no statistics
            _logger.LogInformation($"{nameof(UserInteractions)}: {content}.");
            Console.WriteLine($"\n\n{content}");
        }

        private (string, string) AcceptLoginPassword()
        {
            _logger.LogInformation($"{nameof(UserInteractions)}: Accepting user login and password.");

            var exitWord = "exit";
            var message = "\nEnter your login, please: ";
            var tryAgainMessage = $"\nLogin should contain at least {_settings.LoginMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var login = _userInput.ReadString(message, tryAgainMessage, _settings.LoginMinLength, exitWord);

            if (login == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
                _logger.LogInformation($"{nameof(UserInteractions)}: The authorization is canceled by user.");
                return (null, null);
            }

            message = "\nEnter your password, please: ";
            tryAgainMessage = $"\nPassword should contain at least {_settings.PasswordMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var password = _userInput.ReadString(message, tryAgainMessage, _settings.PasswordMinLength, exitWord);

            if (password == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
                _logger.LogInformation($"{nameof(UserInteractions)}: The authorization is canceled by user.");
                return (null, null);
            }

            _logger.LogInformation($"{nameof(UserInteractions)}: Login and password are accepted.");
            return (login, password.GetDeterministicHashCode().ToString());
        }

        private bool IsBlocked()
        {
            _logger.LogInformation($"{nameof(UserInteractions)}: Checking whether authorization is blocked or not.");

            if (_authBlockedAt == null)
            {
                return false;   // not blocked
            }

            if (DateTime.Now.Subtract((DateTime)_authBlockedAt).TotalSeconds > _settings.AuthorizationBlockingTime)
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
