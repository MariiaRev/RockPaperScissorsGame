using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Client.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client
{
    public class Tests
    {
        private readonly ForAuthorizationAndRegistration _authAndRegistrationService;
        private readonly HttpClient _client;

        public Tests(ForAuthorizationAndRegistration authAndRegistrationService, HttpClient client, IOptions<ClientOptions> options)
        {
            _authAndRegistrationService = authAndRegistrationService;
            _client = client;
            _client.BaseAddress = new Uri(options.Value.BaseAddress);
        }

        public async Task RunAsync()
        {
            var tests = new List<Task>()
            {
                Test_Success_AuthorizationAsync(),
                Test_Fail_AuthorizationAsync(),
                Test_Success_RegistrationAsync(),
                Test_Fail_RegistrationAsync()
            };

            await Task.WhenAll(tests);

            Debug.WriteLine("\n\n\n");
        }

        private async Task<bool> Test_Success_AuthorizationAsync()
        {
            string login = "login1";
            string password = "1111111";
            var testName = "Test_Success_Authorization";

            var token = await _authAndRegistrationService.Authorize(login, password);

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

            if (await _authAndRegistrationService.Authorize(login, password) == null)
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

            (var success, var message) = await _authAndRegistrationService.Register(login, password);
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
            (var success, var message) = await _authAndRegistrationService.Register(login, password);

            if (!success)
            {
                Debug.WriteLine($"\n{message}\n{testName}: Test passed.");
                return true;
            }

            Debug.WriteLine($"\n{message}\n{testName}: Test failed.");
            return false;
        }
    }
}
