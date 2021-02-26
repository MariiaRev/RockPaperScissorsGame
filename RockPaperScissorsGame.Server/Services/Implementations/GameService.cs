using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Exceptions;
using RockPaperScissorsGame.Server.Models.Game;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Services.Implementations
{
    public class GameService : IGameService
    {
        class MoveOptionsPair
        {
            private readonly MoveOptions _moveOption1;
            private readonly MoveOptions _moveOption2;

            public MoveOptionsPair(MoveOptions moveOption1, MoveOptions moveOption2)
            {
                _moveOption1 = moveOption1;
                _moveOption2 = moveOption2;
            }

            public override int GetHashCode()
            {
                return _moveOption1.GetHashCode() + _moveOption2.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                return Equals(obj as MoveOptionsPair);
            }
            
            public bool Equals(MoveOptionsPair obj)
            {
                return obj != null && obj._moveOption1 == this._moveOption1 && obj._moveOption2 == this._moveOption2;
            }
        }
        
        private readonly ILogger<GameService> _logger;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
        }
        
        private readonly Dictionary<MoveOptionsPair, GameOutcome> _gameResults = new Dictionary<MoveOptionsPair, GameOutcome>
        {
            { new MoveOptionsPair(MoveOptions.Rock, MoveOptions.Rock), GameOutcome.Draw },
            { new MoveOptionsPair(MoveOptions.Rock, MoveOptions.Paper), GameOutcome.Loss },
            { new MoveOptionsPair(MoveOptions.Rock, MoveOptions.Scissors), GameOutcome.Win },

            { new MoveOptionsPair(MoveOptions.Paper, MoveOptions.Paper), GameOutcome.Draw },
            { new MoveOptionsPair(MoveOptions.Paper, MoveOptions.Scissors), GameOutcome.Loss },
            { new MoveOptionsPair(MoveOptions.Paper, MoveOptions.Rock), GameOutcome.Win },

            { new MoveOptionsPair(MoveOptions.Scissors, MoveOptions.Scissors), GameOutcome.Draw },
            { new MoveOptionsPair(MoveOptions.Scissors, MoveOptions.Rock), GameOutcome.Loss },
            { new MoveOptionsPair(MoveOptions.Scissors, MoveOptions.Paper), GameOutcome.Win },
        };

        public GameOutcome GetGameResult(MoveOptions playerChoice, MoveOptions opponentChoice)
        {
            MoveOptionsPair figurePair = new MoveOptionsPair(playerChoice, opponentChoice);
            bool isSuccess = _gameResults.TryGetValue(figurePair, out GameOutcome result);

            if (isSuccess)
            {
                _logger.LogInformation($"{nameof(GameService)}: Game result calculated");
                return result;
            }
            _logger.LogError($"{nameof(GameService)}: Invalid pair of figures");

            throw new ServiceException("Invalid pair of figures");
        }
    } 
}
