using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Options;
using RockPaperScissorsGame.Client.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client
{
    public class Tests
    {
        private readonly ForAuthorizationAndRegistration _authAndRegistrationService;
        private readonly HttpClient _client;
        private readonly UserInteractions _userInteractions;

        public Tests(
            ForAuthorizationAndRegistration authAndRegistrationService, 
            HttpClient client,
            IOptions<ClientOptions> options,
            UserInteractions userInteractions)
        {
            _authAndRegistrationService = authAndRegistrationService;
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
            _userInteractions = userInteractions;
        }

        public async Task RunAsync()
        {
            var tests = new List<Task>()
            {
                //Test_Success_AuthorizationAsync(),
                //Test_Fail_AuthorizationAsync(),
                //Test_Success_RegistrationAsync(),
                //Test_Fail_RegistrationAsync()

            };

            //await Task.WhenAll(tests);
            Console.WriteLine("Testing auth-n 1");
            await Test_AuthorizeUserAsync();
            Console.WriteLine("\n\nTesting auth-n 2");
            await Test_AuthorizeUserAsync();

            await Task.Delay(61000);
            Console.WriteLine("\n\nTesting auth-n 3");
            await Test_AuthorizeUserAsync();

            //Console.WriteLine("\n\nTesting registration 1");
            //await Test_RegisterUserAsync();
            //Console.WriteLine("\n\nTesting registration 2");
            //await Test_RegisterUserAsync();

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


    }
}
