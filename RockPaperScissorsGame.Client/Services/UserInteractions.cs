using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Settings;

namespace RockPaperScissorsGame.Client.Services
{
    public class UserInteractions
    {
        private readonly IUserInput _userInputService;
        private readonly UserInfoSettings _options;
        private readonly ForAuthorizationAndRegistration _authRegistrationService;
        private readonly ISingleStorage<string> _authToken;

        private int _authorizationAttempts = 0;
        private DateTime? _authBlockedAt = null;

        public UserInteractions(
            IUserInput userInputService,
            IOptions<UserInfoSettings> options,
            ForAuthorizationAndRegistration authRegistrationService,
            ISingleStorage<string> authToken)
        {
            _userInputService = userInputService;
            _options = options.Value;
            _authRegistrationService = authRegistrationService;
            _authToken = authToken;
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
        public async Task<bool> AuthorizeUserAsync()
        {    
            // check if authorization is blocked
            if (IsBlocked())
            {
                Console.WriteLine("\nAuthorization is still blocked. Please, try again later.");
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
                    var authToken = await _authRegistrationService.AuthorizeAsync(login, password);

                    // if user was not authorized
                    if (authToken == null)
                    {
                        Console.WriteLine("\nWrong login or password.");
                        
                        if (_authorizationAttempts >= _options.AuthorizationAttemptsMax)
                        {
                            //block authorization for a while
                            Console.WriteLine($"\nYou used maximum ({_options.AuthorizationAttemptsMax}) authorization attempts.");
                            Console.WriteLine($"Authorization is blocked for {_options.AuthorizationBlockingTime} seconds.");
                            _authBlockedAt = DateTime.Now;
                            return false;
                        }
                        else
                        {
                            // try again or exit
                            var exitWord = "exit";
                            var message = $"Enter anything to retry authorization or enter '{exitWord}' to exit to the previous menu: ";
                            
                            // if user entered anything but not the exit-word
                            if (!_userInputService.ReadString(message, exitWord, true))
                            {
                                await AuthorizeUserAsync();
                            }

                            // if user entered the exit-word
                            return false;
                        }
                    }

                    // if user was authorized update his/her auth token
                    _authToken.Update(authToken);
                    Console.WriteLine("You are succesfully authorized.");
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
                    (var success, var message ) = await _authRegistrationService.RegisterAsync(login, password);
                    Console.WriteLine($"\n{message}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public void ShowLeaderboardAsync()
        {

        }

        public void ShowUserStatisticsAsync()
        {

        }

        private (string, string) AcceptLoginPassword()
        {
            var exitWord = "exit";
            var message = "\nEnter your login, please: ";
            var tryAgainMessage = $"\nLogin should contain at least {_options.LoginMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var login = _userInputService.ReadString(message, tryAgainMessage, _options.LoginMinLength, exitWord);

            if (login == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
                return (null, null);
            }

            message = "\nEnter your password, please: ";
            tryAgainMessage = $"\nPassword should contain at least {_options.PasswordMinLength} character(s). Try again or enter {exitWord} to exit: ";
            var password = _userInputService.ReadString(message, tryAgainMessage, _options.PasswordMinLength, exitWord);

            if (password == null)
            {
                Console.WriteLine("\nAuthorization is canceled.");
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
