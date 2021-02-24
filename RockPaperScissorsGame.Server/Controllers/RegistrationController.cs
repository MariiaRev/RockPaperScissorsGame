using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using RockPaperScissorsGame.Server.Services;

namespace RockPaperScissorsGame.Server.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IUsersService _users;

        public RegistrationController(
            IUsersService users)
        {
            _users = users;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterAsync(
            [FromHeader(Name ="X-login"), Required] string login,
            [FromHeader(Name = "X-password"), Required] string password)
        {
            if (await _users.SaveAsync(login, password))
            {
                return Ok($"User with login '{login}' was registered.");
            }
            
            return BadRequest($"User with login '{login}' already exists. Try another login.");
        }
    }
}
