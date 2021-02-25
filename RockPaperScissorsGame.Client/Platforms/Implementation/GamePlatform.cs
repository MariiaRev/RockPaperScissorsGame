using System;
using System.Threading.Tasks;
using RockPaperScissorsGame.Client.Exceptions;
using RockPaperScissorsGame.Client.Platforms.Abstract;
using RockPaperScissorsGame.Client.Platforms.Base;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Platforms.Implementation
{
    public class GamePlatform : BasePlatform, IGamePlatform
    {
        private readonly IGameService _gameService;
        private readonly IInGamePlatform _inGamePlatform;

        public GamePlatform(IGameService gameService, IInGamePlatform inGamePlatform)
        {
            _gameService = gameService;
            _inGamePlatform = inGamePlatform;
        }
        
        private async Task FindPublicGame()
        {
            Console.WriteLine("\nFinding opponent...");
            try
            {
                string resultEvent = await _gameService.FindPublicGame(PlayerId);
                if (resultEvent.Equals(GameEvents.GameStart))
                {
                    await _inGamePlatform.StartAsync(0);
                }
                else if (resultEvent.Equals(GameEvents.WaitingForPlayerToJoin))
                {
                    await _inGamePlatform.StartAsync(30);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {
                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        
        private async Task CreatePrivateGame()
        {
            Console.WriteLine("Creating private game...");
            try
            {
                string resultEvent = await _gameService.CreatePrivateRoomAsync(PlayerId);
            
                if (resultEvent.Equals(GameEvents.WaitingForPlayerToJoin))
                {
                    Console.WriteLine("Warning: If nobody will join this game in next 60 seconds it will be automatically closed");
                    await _inGamePlatform.StartAsync(60);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {
                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private async Task JoinPrivateGame()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Enter private room token: ");
                Console.ResetColor();
            
                string token = Console.ReadLine();
                Console.WriteLine("Joining private game...");
                string resultEvent = await _gameService.JoinPrivateRoom(PlayerId, token);
                if (resultEvent.Equals(GameEvents.GameStart))
                {
                    await _inGamePlatform.StartAsync(0);
                }
                else if (resultEvent.Equals(GameEvents.ErrorOccured))
                {
                    // error message handled by handler registered in ConnectionService
                    // just return to the menu
                }
                else
                {
                    throw new UnexpectedBehaviorException("Server returned unexpected event");
                }
            }
            catch (ConnectionException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        
        private void Exit()
        {
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
            Console.WriteLine("0. Exit");
            
            Console.ResetColor();
        }
    }
}