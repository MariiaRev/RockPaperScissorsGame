using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Server.Services;

namespace RockPaperScissorsGame.Server.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IUsersService _users;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            IUsersService users,
            ILogger<RegistrationController> logger)
        {
            _users = users;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterAsync(
            [FromHeader(Name ="X-login"), Required] string login,
            [FromHeader(Name = "X-password"), Required] string password)
        {
            _logger.LogInformation("Request to execute user registration.");

            if (await _users.SaveAsync(login, password))
            {
                _logger.LogInformation($"New user {login} was registered. Return {HttpStatusCode.OK}");
                return Ok($"User with login '{login}' was registered.");
            }

            _logger.LogInformation($"User {login} already exists. Return {HttpStatusCode.BadRequest}");
            return BadRequest($"User with login '{login}' already exists. Try another login.");
        }
    }
}
