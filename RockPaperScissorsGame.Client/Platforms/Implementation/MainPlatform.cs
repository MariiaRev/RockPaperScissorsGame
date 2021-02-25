using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Base;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client.Platforms.Implementation
{
    public class MainPlatform : BasePlatform, IMainPlatform
    {
        private readonly IUserInput _userInputService;
        private readonly UserInfoSettings _options;
        private readonly ISigningService _signingService;
        private readonly ISingleStorage<string> _authToken;
        private readonly IGamePlatform _gamePlatform;
        
        private int _authorizationAttempts;
        private DateTime? _authBlockedAt;
        
        public MainPlatform(
            IUserInput userInputService,
            IOptions<UserInfoSettings> options,
            ISigningService signingService,
            ISingleStorage<string> authToken, 
            IGamePlatform gamePlatform)
        {
            _userInputService = userInputService;
            _options = options.Value;
            _signingService = signingService;
            _authToken = authToken;
            _gamePlatform = gamePlatform;
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
            // check if authorization is blocked
            if (IsBlocked())
            {
                Console.WriteLine($"\n\nAuthorization is still blocked. Please, try again later.");
                return;
            }

            // if authorization is not blocked

            // accept login and password
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                _authorizationAttempts++;
                string authToken = null;
                try
                {
                    authToken = await _signingService.AuthorizeAsync(login, password);
                }
                catch (ConnectionException exception)
                {
                    Console.WriteLine(exception);
                }

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
                        return;
                    }
                        
                    // try again or exit
                    var exitWord = "exit";
                    var message = $"Enter anything to retry authorization or enter '{exitWord}' to exit to the previous menu:";
                            
                    // if user entered anything but not the exit-word
                    if (!_userInputService.ReadString(message, exitWord, true))
                    {
                        await AuthorizeUserAsync();
                    }

                    // if user entered the exit-word
                    return;
                }

                // if user was authorized update his/her auth token
                _authToken.Update(authToken);
                Console.WriteLine("You are successfully authorized.");
                    
                await _gamePlatform.StartAsync(authToken); // move to the next menu
            }
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

        /// <summary>
        /// Interactions with user for registration.
        /// </summary>
        /// <returns>Returns no value.</returns>
        private async Task RegisterUserAsync()
        {
            (var login, var password) = AcceptLoginPassword();

            if (login != null && password != null)
            {
                try
                {
                    (var success, var message ) = await _signingService.RegisterAsync(login, password);
                    Console.WriteLine($"\n\n{message}");
                    if (success)
                    {
                        Console.WriteLine($"User '{login}' was successfully registered");
                    }
                    else
                    {                        
                        Console.WriteLine("Error occured during the registration process. Please, try again later");
                    }
                }
                catch (ConnectionException exception)
                {
                    Console.WriteLine(exception); 
                }
            }
        }
        
        private (string, string) AcceptLoginPassword()
        {
            var exitWord = "exit";
            var message = "\n\nEnter your login, please:";
            var tryAgainMessage = $"\n\nLogin should contain at least {_options.LoginMinLength} character(s). Try again or enter {exitWord} to exit:";
            var login = _userInputService.ReadString(message, tryAgainMessage, _options.LoginMinLength, exitWord);

            if (login == null)
            {
                Console.WriteLine("\n\nAuthorization is canceled.");
                return (null, null);
            }

            message = "\n\nEnter your password, please:";
            tryAgainMessage = $"\n\nPassword should contain at least {_options.PasswordMinLength} character(s). Try again or enter {exitWord} to exit:";
            var password = _userInputService.ReadString(message, tryAgainMessage, _options.PasswordMinLength, exitWord);

            if (password == null)
            {
                Console.WriteLine("\n\nAuthorization is canceled.");
                return (null, null);
            }

            return (login, password);
        }
        
        private async Task ShowLeaderboardAsync()
        {
            Console.WriteLine("TODO");
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