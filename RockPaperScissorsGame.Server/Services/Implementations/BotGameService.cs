using System;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Exceptions;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Services.Implementations
{
    public class BotGameService : IBotGameService
    {
        private readonly ILogger<BotGameService> _logger;

        public BotGameService(ILogger<BotGameService> logger)
        {
            _logger = logger;
        }
        public MoveOptions MakeRandomChoice()
        {
            Array values = Enum.GetValues(typeof(MoveOptions));
            Random random = new Random();

            var randomMoveOption = values.GetValue(random.Next(1, values.Length));
            if (randomMoveOption is MoveOptions moveOption)
            {
                _logger.LogInformation($"{nameof(BotGameService)}: Random move option was selected");
                
                return moveOption;
            }

            _logger.LogError($"{nameof(BotGameService)}: Could not select random move option");

            throw new ServiceException("Could not select random move option");
        }
    }
}