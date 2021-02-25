using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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
        public ActionResult<OfflineRound> Post([FromBody] string userFigureRaw)
        {
            _logger.LogInformation($"{nameof(BotGameController)}: round with bot requested");

            bool isCorrectFigure = Enum.TryParse<MoveOptions>(userFigureRaw, true, out MoveOptions userFigure);
            if (isCorrectFigure == false)
            {
                return BadRequest("Invalid move option");
            }

            MoveOptions botMoveOption = _botGameService.MakeRandomChoice();
            GameOutcome roundResult = _gameService.GetGameResult(userFigure, botMoveOption);
            
            _logger.LogInformation($"{nameof(BotGameController)}: round with bot ended");
            bool doesRequestHaveUserId = Request.Headers.TryGetValue("X-userId", out StringValues rawUserId);
            string userId = string.Empty;
            if (doesRequestHaveUserId)
            {
                userId = rawUserId;
            }
            return new OfflineRound(userId, userFigure, botMoveOption, roundResult);
        }
    }
}