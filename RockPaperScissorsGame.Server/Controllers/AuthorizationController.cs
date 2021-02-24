using System;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Services;

namespace RockPaperScissorsGame.Server.Controllers
{
    [Route("/login")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IStorage<User> _users;
        private readonly IStorage<string> _tokens;

        public AuthorizationController(IStorage<User> users, IStorage<string> tokens)
        {
            _users = users;
            _tokens = tokens;
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromHeader(Name = "X-login"), Required] string login,
                                                    [FromHeader(Name = "X-password"), Required] string password)
        {
            var user = (await _users.GetAllAsync())
                        .Where(user => user.Item.GetLogin() == login && user.Item.VerifyPassword(password))
                        .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var token = await GenerateTokenAsync();
            await _tokens.AddOrUpdateAsync(user.Id, token);

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

            return token;
        }
    }
}
