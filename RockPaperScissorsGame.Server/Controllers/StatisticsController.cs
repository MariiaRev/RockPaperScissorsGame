using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Server.Options;
using RockPaperScissorsGame.Server.Helpers;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Out;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class StatisticsController : ControllerBase
    {
        private readonly IStorage<UserStatistics> _statistics;
        private readonly IStorage<User> _users;
        private readonly StatisticsSettings _options;
        private readonly ILogger<StatisticsController> _logger;
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(
            IStorage<UserStatistics> statistics,
            IStorage<User> users,
            IOptions<StatisticsSettings> options,
            ILogger<StatisticsController> logger,
            IStatisticsService statisticsService)
        {
            _statistics = statistics;
            _users = users;
            _options = options.Value;
            _logger = logger;
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            _logger.LogInformation($"{nameof(StatisticsController)}: Request to get general statistics.");
            var statistics = await _statistics.GetAllAsync();

            if (statistics.Any())
            {
                var users = await _users.GetAllAsync();
                var statisticsOut = statistics
                    .Join(users, st => st.Id, us => us.Id,
                          (stat, user) => ModelsMapper.ToStatisticsOut(user.Item.GetLogin(), stat.Item))
                    .Where(st => st.TotalRoundsCount > _options.MinRoundsCount)
                    .OrderByDescending(st => st.TotalOutcomesCounts.WinsCount);

                if (statisticsOut.Count() > 0)
                {
                    _logger.LogInformation($"{nameof(StatisticsController)}: Show statistics for {statisticsOut.Count()} user(s). Return {HttpStatusCode.OK}");
                    return Ok(statisticsOut);
                }
            }

            _logger.LogInformation($"{nameof(StatisticsController)}: No statisctics to show. Return {HttpStatusCode.OK}");
            return Ok();
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(StatisticsOut), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetUserStatistics(
            [FromHeader(Name = "X-AuthToken"), Required]string token,
            [FromServices, Required] IStorage<string> tokens)
        {
            _logger.LogInformation($"{nameof(StatisticsController)}: Request to get user statistics.");
            var userId = (await tokens.GetAllAsync())
                         .Where(tk => tk.Item == token)
                         .Select(tk => tk.Id)
                         .FirstOrDefault();

            if (userId > 0)     //if user was found
            {
                _logger.LogInformation($"{nameof(StatisticsController)}: User with id {userId} was identified by his/her authorization token.");
                
                // get user statistics
                var statistics = await _statistics.GetAsync(userId);

                if (statistics == null)
                {
                    _logger.LogWarning($"{nameof(StatisticsController)}: There is no statistics (even empty) for the user with id {userId}. Return {HttpStatusCode.NotFound}");
                    return NotFound($"No statistics for the user yet.");
                }

                var userLogin = (await _users.GetAsync(userId)).GetLogin();
                var statisticsOut = ModelsMapper.ToStatisticsOut(userLogin, statistics);
                _logger.LogInformation($"{nameof(StatisticsController)}: Show personal statistics to the user with id {userId}. Return {HttpStatusCode.OK}");

                return Ok(statisticsOut);
            }

            _logger.LogInformation($"{nameof(StatisticsController)}: Authorization token did not exist or expired. User was not identified. Return {HttpStatusCode.Forbidden}");
            return StatusCode((int)HttpStatusCode.Forbidden, "User was not defined. Repeat authorization, please.");
        }

        [HttpPost("/gametime")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> SaveUserInGameTime(
            [FromHeader(Name = "X-AuthToken"), Required] string token,
            [FromHeader(Name = "X-Time"), Required] string gameTime)
        {
            _logger.LogInformation($"{nameof(StatisticsController)}: Request to save user in-game time.");
            var errorMessage = await _statisticsService.SaveGameTime(token, gameTime);
            
            if (errorMessage == null)
            {
                _logger.LogInformation($"{nameof(StatisticsController)}: Successful request for saving user in-game time. Return {HttpStatusCode.OK}.");
                return Ok("In-game time was saved.");
            }

            _logger.LogInformation($"{nameof(StatisticsController)}: {errorMessage} Return {HttpStatusCode.Forbidden}");
            return StatusCode((int)HttpStatusCode.Forbidden, errorMessage);
        }

    }
}
