using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Services;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Controllers
{
    [Route("/login")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IStorage<User> _users;
        private readonly IStorage<string> _tokens;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IStorage<User> users,
            IStorage<string> tokens,
            ILogger<AuthorizationController> logger)
        {
            _users = users;
            _tokens = tokens;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromHeader(Name = "X-login"), Required] string login,
                                                    [FromHeader(Name = "X-password"), Required] string password)
        {
            _logger.LogInformation($"{nameof(AuthorizationController)}: Request to execute user login.");

            var user = (await _users.GetAllAsync())
                        .Where(user => user.Item.GetLogin() == login && user.Item.VerifyPassword(password))
                        .FirstOrDefault();

            if (user == null)
            {
                _logger.LogInformation($"{nameof(AuthorizationController)}: Wrong login or password. User {login} is unauthorized. Return {HttpStatusCode.Unauthorized}");
                return Unauthorized();
            }

            var token = await GenerateTokenAsync();
            await _tokens.AddOrUpdateAsync(user.Id, token);

            _logger.LogInformation($"{nameof(AuthorizationController)}: Correct login and password. User {login} is authorized. Return {HttpStatusCode.OK}");
            return Ok(token);
        }

        private async Task<string> GenerateTokenAsync()
        {
            var tokens = await _tokens.GetAllAsync();
            string token;

            do
            {
                token = Guid.NewGuid().ToString();
            }
            while (tokens.Where(item => item.Item == token).Any());

            _logger.LogInformation($"{nameof(AuthorizationController)}: The authorization token was generated.");
            return token;
        }
    }
}
