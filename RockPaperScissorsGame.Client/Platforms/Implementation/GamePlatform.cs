using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Helpers.Abstract;
using RockPaperScissorsGame.Client.Models;
using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Base;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Platforms.Implementation
{
    public class GamePlatform : BasePlatform, IGamePlatform
    {
        private readonly IGameService _gameService;
        private readonly IStatisticsService _statisticsService;
        private readonly IInGamePlatform _inGamePlatform;
        private readonly ISingleStorage<AuthInfo> _authInfo;
        private readonly ILogger<GamePlatform> _logger;

        public GamePlatform(IGameService gameService, 
                            IStatisticsService statisticsService,
                            IInGamePlatform inGamePlatform, 
                            ISingleStorage<AuthInfo> authInfo,
                            ILogger<GamePlatform> logger)
        {
            _gameService = gameService;
            _statisticsService = statisticsService;
            _inGamePlatform = inGamePlatform;
            _authInfo = authInfo;
            _logger = logger;
        }
        
        private async Task FindPublicGame()
        {
            _logger.LogInformation($"{nameof(GamePlatform)}: Start finding of public room");
            Console.WriteLine("\nLooking for opponent...");
            try
            {
                string resultEvent = await _gameService.FindPublicGame(PlayerId);
                if (resultEvent.Equals(GameEvents.GameStart))
                {
                    _logger.LogInformation($"{nameof(GamePlatform)}: Server added user to public room with player in it");
                    await _inGamePlatform.StartAsync(0);
                }
                else if (resultEvent.Equals(GameEvents.WaitingForPlayerToJoin))
                {
                    _logger.LogInformation($"{nameof(GamePlatform)}: Server added user to a empty public room");
                    await _inGamePlatform.StartAsync(30);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {                    
                    _logger.LogError($"{nameof(GamePlatform)}: Server returned unexpected event");
                    
                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                _logger.LogError($"{nameof(GamePlatform)}: Unable to connect to the server");

                Console.WriteLine(exception.Message);
            }
        }
        
        private async Task CreatePrivateGame()
        {
            _logger.LogInformation($"{nameof(GamePlatform)}: Creating of private room started");
            Console.WriteLine("Creating private game...");
            try
            {
                string resultEvent = await _gameService.CreatePrivateRoomAsync(PlayerId);
            
                if (resultEvent.Equals(GameEvents.WaitingForPlayerToJoin))
                {
                    Console.WriteLine("Warning: If nobody will join this game in next 60 seconds it will be automatically closed\n" +
                                      "Waiting for the opponent...");
                    _logger.LogInformation($"{nameof(GamePlatform)}: Empty private room is created");

                    await _inGamePlatform.StartAsync(60);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {
                    _logger.LogError($"{nameof(GamePlatform)}: Server returned unexpected event");

                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                _logger.LogError($"{nameof(GamePlatform)}: Unable to connect to the server");

                Console.WriteLine(exception.Message);
            }
        }

        private async Task JoinPrivateGame()
        {
            try
            {
                _logger.LogInformation($"{nameof(GamePlatform)}: Connection to te private room starts");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Enter private room token: ");
                Console.ResetColor();
            
                string token = Console.ReadLine();
                Console.WriteLine("Joining private game...");
                string resultEvent = await _gameService.JoinPrivateRoom(PlayerId, token);
                if (resultEvent.Equals(GameEvents.GameStart))
                {
                    _logger.LogInformation($"{nameof(GamePlatform)}: Joined to the desired private room");
                    await _inGamePlatform.StartAsync(0);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {
                    _logger.LogError($"{nameof(GamePlatform)}: Server returned unexpected event");

                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                _logger.LogError($"{nameof(GamePlatform)}: Unable to connect to the server");

                Console.WriteLine(exception.Message);
            }
        }
        
        private async Task PlayWithBot()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("1. Rock");
            Console.WriteLine("2. Paper"); 
            Console.WriteLine("3. Scissors");
            Console.ResetColor();

            MoveOptions figure;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Choose your figure: ");
                Console.ResetColor();
                
                string userInput = Console.ReadLine();
                
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Empty input. Try again\n");
                    continue;
                }

                if (!int.TryParse(userInput.Trim(), out int figureNumber) || figureNumber < 1 || figureNumber > 3 || Enum.TryParse(userInput, out figure) == false)
                {
                    Console.WriteLine("Unknown figure. Try again\n");
                    continue;
                }
                
                break;
            }

            try
            {
                string roundResult = await _gameService.PlayRoundWithBot(PlayerId, figure);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(roundResult);
                Console.ResetColor();
            }
            catch (Exception exception)
            {
                if (exception is ConnectionException ||
                    exception is ServiceException)
                {
                    Console.WriteLine(exception.Message);
                }
                else
                {
                    throw;
                }
            }
        }
        
        private async Task ShowPersonalStatisticsAsync()
        {
            var authInfo = _authInfo.Get();
            if (authInfo == null || authInfo?.Token == null)
            {
                Console.WriteLine("\n\nPlease, authorize before viewing personal statistics.");
                _logger.LogInformation($"{nameof(MainPlatform)}: The user is unathorized and cannot view personal statistics.");
                return;
            }

            // if user is authorized
            // ask to send request to save user in-game time
            await _statisticsService.SaveUserGameTime();

            // ask to send request for statistics
            (var success, var content) = await _statisticsService.GetUserStatisticsAsync();

            if (success)
            {
                try
                {
                    _logger.LogInformation($"{nameof(MainPlatform)}: The user statistics is recieved.");

                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<UserStatistics>(content);
                    _logger.LogInformation($"{nameof(MainPlatform)}: The user statistics is deserialized and shown.");
                    Console.WriteLine($"\n\n{" ", 4}$$$ Your statistics $$$\n\n{statistics}");
                    Console.WriteLine("-----------------------------------------------------------------");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    _logger.LogInformation($"{nameof(MainPlatform)}: Exception during the user statistics deserialization from json.");
                }

                return;
            }

            // if no statistics
            _logger.LogInformation($"{nameof(MainPlatform)}: {content}.");
            Console.WriteLine($"\n\n{content}");
        }
        
        private async Task ShowLeaderboardAsync()
        {
            _logger.LogInformation($"{nameof(MainPlatform)}: Attempt to view the leaderboard.");

            // if user is authorized save his/her in-game time
            var authInfo = _authInfo.Get();
            if (authInfo != null && authInfo?.Token != null)
            {
                // ask to send request to save user in-game time
                await _statisticsService.SaveUserGameTime();
            }

            // ask to send request for statistics
            bool success;
            string content;
            try
            {
                (success, content) = await _statisticsService.GetLeaderboardAsync();
            }
            catch (ConnectionException exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }

            if (success)
            {
                try
                {
                    _logger.LogInformation($"{nameof(MainPlatform)}: The leaderboard is recieved.");

                    // try deserialise json string (content) into list of UserStatistics
                    var statistics = JsonConvert.DeserializeObject<List<UserStatistics>>(content);
                    _logger.LogInformation($"{nameof(MainPlatform)}: The leaderboard is deserialised.");

                    if (statistics == null)
                    {
                        Console.WriteLine($"\n\nNo statistics in the leaderboard yet.");
                        return;
                    }

                    // show leaderboard
                    Console.WriteLine($"\n\n{" ", 4}$$$ LEADERBOARD $$$\n");

                    foreach(var userStatistics in statistics)
                    {
                        Console.WriteLine($"{userStatistics}");
                    }

                    Console.WriteLine("-----------------------------------------------------------------");
                    _logger.LogInformation($"{nameof(MainPlatform)}: Statistics for {statistics.Count} user(s) was shown.");
                }
                catch (JsonSerializationException)
                {
                    Console.WriteLine($"\n\nWe're sorry, an error occured. Statistics is temporarily unavailable.");
                    _logger.LogInformation($"{nameof(MainPlatform)}: Exception during the leaderboard deserialization from json.");
                }

                return;
            }

            // if no statistics
            Console.WriteLine($"\n\n{content}");
            _logger.LogInformation($"{nameof(MainPlatform)}: {content}.");
        }
        
        private void Exit()
        {
            _statisticsService.SaveUserGameTime();
            _authInfo.Get().Watch.Reset();
            KeepProgramActive = false;
        }
        
        protected override async Task<bool> ChooseCommandAsync(int commandNumber)
        {
            bool correctCommand = true;
            switch (commandNumber)
            {
                case 1:
                    await FindPublicGame();
                    break;
                case 2:
                    await CreatePrivateGame();
                    break;
                case 3:
                    await JoinPrivateGame();
                    break;
                case 4:
                    await PlayWithBot();
                    break;
                case 5:
                    await ShowPersonalStatisticsAsync();
                    break;
                case 6:
                    await ShowLeaderboardAsync();
                    break;
                case 0:
                    Exit();
                    break;
                default:
                    correctCommand = false;
                    break;
            }

            return correctCommand;
        }
        
        protected override async Task PrintUserMenu()
        {
            await Task.Delay(500);
            Console.ForegroundColor = ConsoleColor.Green;
            
            Console.WriteLine();
            Console.WriteLine("1. Find public game");
            Console.WriteLine("2. Create private game");
            Console.WriteLine("3. Join private game");
            Console.WriteLine("4. Play with bot");
            Console.WriteLine("5. Personal statistics");
            Console.WriteLine("6. Leaderboard");
            Console.WriteLine("0. Exit");
            
            Console.ResetColor();
        }
    }
}