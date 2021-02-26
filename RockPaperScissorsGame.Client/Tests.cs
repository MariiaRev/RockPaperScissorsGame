using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Services;

namespace RockPaperScissorsGame.Client
{
    public class Tests
    {
        private readonly ForAuthorizationAndRegistration _authAndRegistrationService;
        private readonly HttpClient _client;
        private readonly UserInteractions _userInteractions;
        private readonly RequestsForStatistics _requestsForStatistics;

        public Tests(
            ForAuthorizationAndRegistration authAndRegistrationService, 
            HttpClient client,
            IOptions<ClientOptions> options,
            UserInteractions userInteractions,
            RequestsForStatistics requestsForStatistics)
        {
            _authAndRegistrationService = authAndRegistrationService;
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
            _userInteractions = userInteractions;
            _requestsForStatistics = requestsForStatistics;
        }

        public async Task RunAsync()
        {
            #region Tests for requests to the server for login and registration
            //var tests1 = new List<Task>()
            //{
            //    //Test_Success_AuthorizationAsync(),
            //    //Test_Fail_AuthorizationAsync(),
            //    //Test_Success_RegistrationAsync(),
            //    //Test_Fail_RegistrationAsync()

            //};

            //await Task.WhenAll(tests1);
            #endregion

            #region Tests for login and registration with user interactions
            //Console.WriteLine("Testing auth-n 1");
            //await Test_AuthorizeUserAsync();
            //Console.WriteLine("\n\nTesting auth-n 2");
            //await Test_AuthorizeUserAsync();
            //await Task.Delay(61000);
            //Console.WriteLine("\n\nTesting auth-n 3");
            //await Test_AuthorizeUserAsync();
            //Console.WriteLine("\n\nTesting registration 1");
            //await Test_RegisterUserAsync();
            //Console.WriteLine("\n\nTesting registration 2");
            //await Test_RegisterUserAsync();
            #endregion

            #region Tests for requests to the server for statistics
            //var tests2 = new List<(bool, string)>()
            //{
            //    await Test_GetLeaderboard(),
            //    await Test_GetUserStatistics(),
            //    await Test_GetUserStatistics()
            //};
            #endregion

            #region Tests for requests to the server for statistics with user interactions
            await _userInteractions.AuthorizeUserAsync();
            await _userInteractions.ShowLeaderboardAsync();
            await Task.Delay(60000);
            await _userInteractions.ShowUserStatisticsAsync();
            #endregion

            Debug.WriteLine("\n\n\n");
        }

        private async Task<bool> Test_Success_AuthorizationAsync()
        {
            string login = "login1";
            string password = "1111111";
            var testName = "Test_Success_Authorization";

            var token = await _authAndRegistrationService.AuthorizeAsync(login, password);

            if (token != null)
            {
                Debug.WriteLine($"\nToken received: {token}\n{testName}: Test passed.");
                return true;
            }

            Debug.WriteLine($"\n{testName}: Test failed.");
            return false;
        }

        private async Task<bool> Test_Fail_AuthorizationAsync()
        {
            string login = "login1";
            string password = "222222";
            var testName = "Test_Fail_Authorization";

            if (await _authAndRegistrationService.AuthorizeAsync(login, password) == null)
            {
                Debug.WriteLine($"\n{testName}: Test passed.");
                return true;
            }

            Debug.WriteLine($"\n{testName}: Test failed.");
            return false;
        }

        private async Task<bool> Test_Success_RegistrationAsync()
        {
            string login = $"login_{DateTime.Now}";
            string password = "some_password";
            var testName = "Test_Success_Registration";

            (var success, var message) = await _authAndRegistrationService.RegisterAsync(login, password);
            if (success)
            {
                Debug.WriteLine($"\n{message}\n{testName}: Test passed.");
                return true;
            }

            Debug.WriteLine($"\n{message}\n{testName}: Test failed.");
            return false;
        }

        private async Task<bool> Test_Fail_RegistrationAsync()
        {
            string login = "login1";
            string password = "doesn't matter";
            var testName = "Test_Fail_Registration";
            (var success, var message) = await _authAndRegistrationService.RegisterAsync(login, password);

            if (!success)
            {
                Debug.WriteLine($"\n{message}\n{testName}: Test passed.");
                return true;
            }

            Debug.WriteLine($"\n{message}\n{testName}: Test failed.");
            return false;
        }

        private async Task Test_AuthorizeUserAsync()
        {
            await _userInteractions.AuthorizeUserAsync();
        }

        private async Task Test_RegisterUserAsync()
        {
            await _userInteractions.RegisterUserAsync();
        }

        private async Task<(bool, string)> Test_GetLeaderboard()
        {
            var result = await _requestsForStatistics.GetLeaderboardAsync();
            Console.WriteLine($"\n\nLeaderboard request:\n{result.Item2}");
            return result;
        }

        private async Task<(bool, string)> Test_GetUserStatistics()
        {
            try
            {
                // if user is authorized
                if (await _userInteractions.AuthorizeUserAsync())
                {
                    var result = await _requestsForStatistics.GetUserStatisticsAsync();
                    Console.WriteLine($"\n\nUser statistics:\n{result.Item2}");
                    return result;
                }

                Console.WriteLine("\n\nUser is unauthorized.");
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("\n\nUnathorized user tried to access personal statistics!");
            }
            
            return (false, null);
        }
    }
}
