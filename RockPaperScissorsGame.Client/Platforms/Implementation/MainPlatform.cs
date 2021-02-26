using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Helpers.Implementations;
using RockPaperScissorsGame.Client.Models;
using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Base;
using RockPaperScissorsGame.Client.Services;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client.Platforms.Implementation
{
    public class MainPlatform : BasePlatform, IMainPlatform
    {
        private readonly IUserInput _userInputService;
        private readonly UserInfoSettings _settings;
        private readonly ISigningService _signingService;
        private readonly IStatisticsService _statisticsService;
        private readonly ISingleStorage<AuthInfo> _authInfo;
        private readonly IGamePlatform _gamePlatform;
        private readonly ILogger<MainPlatform> _logger;

        private int _authorizationAttempts;
        private DateTime? _authBlockedAt;
        
        public MainPlatform(
            IUserInput userInputService,
            IOptions<UserInfoSettings> options,
            ISigningService signingService,
            IGamePlatform gamePlatform,
            ILogger<MainPlatform> logger, 
            ISingleStorage<AuthInfo> authInfo, 
            IStatisticsService statisticsService)
        {
            _userInputService = userInputService;
            _settings = options.Value;
            _signingService = signingService;
            _gamePlatform = gamePlatform;
            _logger = logger;
            _authInfo = authInfo;
            _statisticsService = statisticsService;
        }
        
        /// <summary>
        /// Interactions with user for authorization.
        /// Blocks authorization after <see cref="UserInfoSettings.AuthorizationAttemptsMax"/> unsuccessfull 
        /// authorization attempts for <see cref="UserInfoSettings.AuthorizationBlockingTime"/> seconds.
        /// </summary>
        /// <returns>
        /// True if user was authorized.
        /// False if user was not authorized.
        /// </returns>
        private async Task AuthorizeUserAsync()
        {    
            _logger.LogInformation($"{nameof(MainPlatform)}: Attempt for user authorization.");

            // check if authorization is blocked
            if (IsBlocked())
            {
                _logger.LogInformation($"{nameof(MainPlatform)}: Authorization is blocked.");
                Console.WriteLine($"\nAuthorization is still blocked. Please, try again later.");
                return;
            }

            // if authorization is not blocked
            _logger.LogInformation($"{nameof(MainPlatform)}: Authorization is not blocked.");

            // accept login and password
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                
                _authorizationAttempts++;
                _logger.LogInformation($"{nameof(MainPlatform)}: The number of authorization attempts has been increased.");
                string authToken = null;    
                try
                {
                    authToken = await _signingService.AuthorizeAsync(login, password);
                }
                catch (ConnectionException exception)
                {
                    Console.WriteLine(exception.Message);
                    return;
                }

                // if user was not authorized
                if (authToken == null)
                {
                    _logger.LogInformation($"{nameof(MainPlatform)}: Wrong login or/and password.");
                    Console.WriteLine("\nWrong login or password.");
                        
                    if (_authorizationAttempts >= _settings.AuthorizationAttemptsMax)
                    {
                        //block authorization for a while
                        Console.WriteLine($"\nYou used maximum ({_settings.AuthorizationAttemptsMax}) authorization attempts.");
                        Console.WriteLine($"Authorization is blocked for {_settings.AuthorizationBlockingTime} seconds.");
                        _authBlockedAt = DateTime.Now;
                        _logger.LogInformation($"{nameof(UserInteractions)}: Authorization was blocked at {_authBlockedAt} for " +
                                               $"{_settings.AuthorizationBlockingTime} seconds after {_authorizationAttempts} unsuccessful attempts.");
                        return;
                    }
                        
                    // try again or exit
                    var exitWord = "exit";
                    var message = $"Enter anything to retry authorization or enter '{exitWord}' to exit to the previous menu: ";
                    
                    // if user entered anything but not the exit-word
                    if (!_userInputService.ReadString(message, exitWord, true))
                    {
                        _logger.LogInformation($"{nameof(MainPlatform)}: User chose one more authorization attempt.");
                        await AuthorizeUserAsync();
                    }

                    // if user entered the exit-word
                    _logger.LogInformation($"{nameof(MainPlatform)}: The authorization is canceled by user.");
                    return;
                }

                
                
                // if user was authorized update his/her auth token
                await _authInfo.UpdateAsync(new AuthInfo(authToken));
                _logger.LogInformation($"{nameof(ForAuthorizationAndRegistration)}: A new authorization token was assigned to the user.");

                Console.WriteLine("\nYou are successfully authorized.");
                _logger.LogInformation($"{nameof(MainPlatform)}: Successful authorization of the user.");
                
                await _gamePlatform.StartAsync(authToken); // move to the next menu
            }
        }
        
        private bool IsBlocked()
        {
            _logger.LogInformation($"{nameof(MainPlatform)}: Checking whether authorization is blocked or not.");

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

        /// <summary>
        /// Interactions with user for registration.
        /// </summary>
        /// <returns>Returns no value.</returns>
        private async Task RegisterUserAsync()
        {
            _logger.LogInformation($"{nameof(MainPlatform)}: Attempt for registration a new user.");
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    (var success, var message ) = await _signingService.RegisterAsync(login, password);
                    Console.WriteLine($"\n{message}");
                    _logger.LogInformation($"{nameof(MainPlatform)}: {message}.");
                }
                catch (ConnectionException exception)
                {
                    Console.WriteLine(exception.Message); 
                }
            }
        }
        
        private (string, string) AcceptLoginPassword()
        {
            _logger.LogInformation($"{nameof(MainPlatform)}: Accepting user login and password.");

            var exitWord = "exit";
            var message = "\nEnter your login, please: ";
            var tryAgainMessage = $"\nLogin should contain at least {_settings.LoginMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var login = _userInputService.ReadString(message, tryAgainMessage, _settings.LoginMinLength, exitWord);

            if (login == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
                _logger.LogInformation($"{nameof(MainPlatform)}: The authorization is canceled by user.");
                return (null, null);
            }

            message = "\nEnter your password, please: ";
            tryAgainMessage = $"\nPassword should contain at least {_settings.PasswordMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var password = _userInputService.ReadString(message, tryAgainMessage, _settings.PasswordMinLength, exitWord);

            if (password == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
                _logger.LogInformation($"{nameof(MainPlatform)}: The authorization is canceled by user.");
                return (null, null);
            }

            _logger.LogInformation($"{nameof(MainPlatform)}: Login and password are accepted.");
            return (login, password.GetDeterministicHashCode().ToString());
        }
        
        private async Task ShowLeaderboardAsync()
        {
            _logger.LogInformation($"{nameof(MainPlatform)}: Attempt to view the leaderboard.");

            // if user is authorized save his/her in-game time
            var authInfo = _authInfo.Get();
            if (authInfo != null && authInfo?.Token != null)
            {
                // ask to send request to save user in-game time
                await _statisticsService.SaveUserGameTime();
            }

            // ask to send request for statistics
            bool success;
            string content;
            try
            {
                (success, content) = await _statisticsService.GetLeaderboardAsync();
            }
            catch (ConnectionException exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }

            if (success)
            {
                try
                {
                    _logger.LogInformation($"{nameof(MainPlatform)}: The leaderboard is recieved.");

                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<List<UserStatistics>>(content);
                    _logger.LogInformation($"{nameof(MainPlatform)}: The leaderboard is deserialised.");

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
                    _logger.LogInformation($"{nameof(MainPlatform)}: Statistics for {statistics.Count} user(s) was shown.");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    _logger.LogInformation($"{nameof(MainPlatform)}: Exception during the leaderboard deserialization from json.");
                }

                return;
            }

            // if no statistics
            Console.WriteLine($"\n\n{content}");
            _logger.LogInformation($"{nameof(MainPlatform)}: {content}.");
        }

        private void Exit()
        {
            KeepProgramActive = false;
        }
        
        protected override async Task<bool> ChooseCommandAsync(int commandNumber)
        {
            bool correctCommand = true;
            switch (commandNumber)
            {
                case 1:
                    await AuthorizeUserAsync();
                    break;
                case 2:
                    await RegisterUserAsync();
                    break;
                case 3:
                    await ShowLeaderboardAsync();
                    break;
                case 0:
                    Exit();
                    break;
                default:
                    correctCommand = false;
                    break;
            }

            return correctCommand;
        }

        protected override async Task PrintUserMenu()
        {
            await Task.Delay(500);
            Console.ForegroundColor = ConsoleColor.Green;
            
            Console.WriteLine();
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Leaderboard");
            Console.WriteLine("0. Exit");
            
            Console.ResetColor();
        }
    }
}