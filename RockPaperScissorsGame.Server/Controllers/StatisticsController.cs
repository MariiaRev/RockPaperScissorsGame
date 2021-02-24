using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using RockPaperScissorsGame.Server.Services;
using RockPaperScissorsGame.Server.Options;
using RockPaperScissorsGame.Server.Helpers;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Out;

namespace RockPaperScissorsGame.Server.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class StatisticsController : ControllerBase
    {
        private readonly IStorage<UserStatistics> _statistics;
        private readonly IStorage<User> _users;
        private readonly StatisticsOptions _options;

        public StatisticsController(
            IStorage<UserStatistics> statistics,
            IStorage<User> users,
            IOptions<StatisticsOptions> options)
        {
            _statistics = statistics;
            _users = users;
            _options = options.Value;
        }

        [HttpGet]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            var statistics = await _statistics.GetAllAsync();

            if (statistics.Any())
            {
                var users = await _users.GetAllAsync();
                var statisticsOut = statistics
                    .Join(users, st => st.Id, us => us.Id,
                          (stat, user) => ModelsMapper.ToStatisticsOut(user.Item.GetLogin(), stat.Item))
                    .Where(st => st.TotalRoundsCount > _options.MinRoundsCount);

                return Ok(statisticsOut);
            }

            return Ok("No statistics yet.");
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserStatistics(
            [FromHeader(Name = "X-AuthToken"), Required]string token,
            [FromServices] IStorage<string> tokens)
        {
            var userId = (await tokens.GetAllAsync())
                         .Where(tk => tk.Item == token)
                         .Select(tk => tk.Id)
                         .FirstOrDefault();

            if (userId > 0)     //if user was found
            {
                // get user statistics
                var statistics = await _statistics.GetAsync(userId);

                if (statistics == null)
                    return NotFound($"No statistics for the user yet.");

                var userLogin = (await _users.GetAsync(userId)).GetLogin();
                var statisticsOut = ModelsMapper.ToStatisticsOut(userLogin, statistics);

                return Ok(statisticsOut);
            }

            return NotFound("User was not defined. Repeat authorization, please.");
        }
    }
}
