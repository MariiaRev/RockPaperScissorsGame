using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            IStorage<UserStatistics> statistics,
            IStorage<User> users,
            IOptions<StatisticsOptions> options,
            ILogger<StatisticsController> logger)
        {
            _statistics = statistics;
            _users = users;
            _options = options.Value;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            _logger.LogInformation("Request to get general statistics.");
            var statistics = await _statistics.GetAllAsync();

            if (statistics.Any())
            {
                var users = await _users.GetAllAsync();
                var statisticsOut = statistics
                    .Join(users, st => st.Id, us => us.Id,
                          (stat, user) => ModelsMapper.ToStatisticsOut(user.Item.GetLogin(), stat.Item))
                    .Where(st => st.TotalRoundsCount > _options.MinRoundsCount);

                _logger.LogInformation($"Show statistics of {statisticsOut.Count()} users. Return {HttpStatusCode.OK}");
                return Ok(statisticsOut);
            }

            _logger.LogInformation($"No statisctics to show. Return {HttpStatusCode.OK}");
            return Ok("No statistics yet.");
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserStatistics(
            [FromHeader(Name = "X-AuthToken"), Required]string token,
            [FromServices] IStorage<string> tokens)
        {
            _logger.LogInformation("Request to get user statistics.");
            var userId = (await tokens.GetAllAsync())
                         .Where(tk => tk.Item == token)
                         .Select(tk => tk.Id)
                         .FirstOrDefault();

            if (userId > 0)     //if user was found
            {
                _logger.LogInformation($"User with id {userId} was identified by his/her authorization token.");
                // get user statistics
                var statistics = await _statistics.GetAsync(userId);

                if (statistics == null)
                {
                    _logger.LogWarning($"There is no statistics (even empty) for the user with id {userId}. Return {HttpStatusCode.NotFound}");
                    return NotFound($"No statistics for the user yet.");
                }

                var userLogin = (await _users.GetAsync(userId)).GetLogin();
                var statisticsOut = ModelsMapper.ToStatisticsOut(userLogin, statistics);
                _logger.LogInformation($"Show personal statistics to the user with id  {userId}. Return {HttpStatusCode.OK}");

                return Ok(statisticsOut);
            }

            _logger.LogInformation($"Authorization token did not exist or expired. User was not identified. Return {HttpStatusCode.NotFound}");
            return NotFound("User was not defined. Repeat authorization, please.");
        }
    }
}
