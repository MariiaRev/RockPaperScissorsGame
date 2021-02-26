using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Models.Game;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Controllers
{
    [Route("/bot")]
    [ApiController]
    public class BotGameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IBotGameService _botGameService;
        private readonly ILogger<BotGameController> _logger;

        public BotGameController(IGameService gameService, IBotGameService botGameService, ILogger<BotGameController> logger)
        {
            _gameService = gameService;
            _botGameService = botGameService;
            _logger = logger;
        }
        
        [HttpPost]
        [Route("play")]
        [ProducesResponseType(typeof(RoundWithBotResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        public  async Task<ActionResult<RoundWithBotResult>>  Post([FromHeader(Name = "X-AuthToken"), Required] string userToken, 
                                              [FromBody] string userFigureRaw,
                                              [FromServices] IStorage<string> tokens)
        {
            _logger.LogInformation($"{nameof(BotGameController)}: Round with bot requested");
            var userId = (await tokens.GetAllAsync())
                         .Where(tk => tk.Item == userToken)
                         .Select(tk => tk.Id)
                         .FirstOrDefault();

            if (userId > 0)     //if user was found
            {
                bool isCorrectFigure = Enum.TryParse<MoveOptions>(userFigureRaw, true, out MoveOptions userFigure);
                if (isCorrectFigure == false)
                {
                    return BadRequest("Invalid move option");
                }

                MoveOptions botMoveOption = _botGameService.MakeRandomChoice();
                GameOutcome roundResult = _gameService.GetGameResult(userFigure, botMoveOption);

                _logger.LogInformation($"{nameof(BotGameController)}: Round with bot ended");

                RoundWithBotResult result = new RoundWithBotResult()
                {
                    UserMoveOption = userFigure,
                    BotMoveOption = botMoveOption,
                    RoundResult = roundResult.ToString()
                };
                var jsonResult = JsonSerializer.Serialize(result);
                return Ok(jsonResult);
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Unauthorized access");
        }
    }
}