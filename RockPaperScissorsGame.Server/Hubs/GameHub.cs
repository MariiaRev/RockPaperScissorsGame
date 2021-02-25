using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Models.Game;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IGameStoringService _gameStorageService;
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(IGameService gameService, IGameStoringService gameStorage, IStatisticsService statisticsService, ILogger<GameHub> logger)
        {
            _gameService = gameService;
            _gameStorageService = gameStorage;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        private string GetUserToken()
        {
            var httpContext = Context.GetHttpContext();
            var token = httpContext.Request.Query["AuthToken"];
            return token.ToString();
        }

        public async Task<string> CreatePrivateRoom()
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Private room creation requested");

            string roomToken = _gameStorageService.GenerateRoomToken();

            bool isSuccess = _gameStorageService.CreateRoom(roomToken, true, userId);
            if (isSuccess)
            {
                string welcomingMessage = "Private game successfully created\n" +
                                          $"Private game token: {roomToken}";
                
                await Groups.AddToGroupAsync(Context.ConnectionId, roomToken);
                await SendMessageToCaller(welcomingMessage);
                
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Private room created");
                return GameEvents.WaitingForPlayerToJoin;
            }

            string failMessage = "Sorry, game wasn't created. Try again later";
            await SendMessageToCaller(failMessage);
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Private room wasn't created");

            return GameEvents.ErrorOccured;
        }
        
        public async Task<string> FindPublicRoom()
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Search for public room requested");

            string publicRoomToken = _gameStorageService.FindPublicRoom();
                
            if (string.IsNullOrWhiteSpace(publicRoomToken))
            {
                return await CreatePublicRoom();
            }

            bool isSuccess = _gameStorageService.JoinRoom(publicRoomToken, userId);
            if (!isSuccess)
            {
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Error occured while searching for public room");
                string failMessage = "Error occured while finding an opponent. Try again later";
                await SendMessageToCaller(failMessage);
                
                return GameEvents.ErrorOccured;
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, publicRoomToken);
            
            await SendMessageToGroupExceptCaller(publicRoomToken, "Opponent joined the game");
            await SendMessageToGroup(publicRoomToken, "Game is ready!");
            
            await Clients.Group(publicRoomToken).SendAsync(GameEvents.GameStart);
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Public room successfully found");

            return GameEvents.GameStart;
        }
        
        private async Task<string> CreatePublicRoom()
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Public room creation called");

            string roomToken = _gameStorageService.GenerateRoomToken();
            
            bool isSuccess = _gameStorageService.CreateRoom(roomToken, false, userId);
            
            if (isSuccess)
            {
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Public room created");
                string welcomingMessage = "Waiting for the opponent...";
                await Groups.AddToGroupAsync(Context.ConnectionId, roomToken);
                await SendMessageToCaller(welcomingMessage);
                
                return GameEvents.WaitingForPlayerToJoin;
            }
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Public room wasn't created");
            string failMessage = "Sorry, error occured while trying to find an opponent. Try again later";
            await SendMessageToCaller(failMessage);
                
            return GameEvents.ErrorOccured;
        }
        
        public async Task<string> JoinPrivateRoom(string roomToken)
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Private room joining was requested");

            if (_gameStorageService.IsPrivateRoomExists(roomToken))
            {
                bool isSuccess = _gameStorageService.JoinRoom(roomToken, userId);
                if (!isSuccess)
                {
                    string failMessage = "Error occured while joining private game. Try again later";
                    
                    await SendMessageToCaller(failMessage);
                    _logger.LogInformation($"{nameof(GameHub)} | {userId}: Error occured while joining private game");

                    return GameEvents.ErrorOccured;
                }
                
                await Groups.AddToGroupAsync(Context.ConnectionId, roomToken);
                await SendMessageToGroupExceptCaller(roomToken, "Opponent joined the game");
                await Clients.Group(roomToken).SendAsync(GameEvents.GameStart);
                await SendMessageToGroup(roomToken, "Game is ready!");
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Successfully connected to the private game");

                return GameEvents.GameStart;
            }
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Non-existent private game token was received");
            string message = "Private game with this token doesn't exist";
            await SendMessageToCaller(message);
                
            return GameEvents.ErrorOccured;
        }

        public async Task<string> MakeMove(string choice)
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;

            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move processing was requested");

            GameRoom currentRoom = _gameStorageService.GetRoomByPlayer(userId);
            if (currentRoom == null)
            {
                string failMessage = "You are not allowed to make moves in this game";
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move processing was denied because user is not allowed to make moves in this game");
                await SendMessageToCaller(failMessage);

                return GameEvents.ErrorOccured;
            }
            
            Series currentRound = currentRoom.Series;
            if (currentRound == null)
            {
                string failMessage = "Error occured. Try again later";
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move processing was denied because there is no Series in GameRoom");
                await SendMessageToCaller(failMessage);

                return GameEvents.ErrorOccured;
            }
            
            bool isSuccess = Enum.TryParse(choice, true, out MoveOptions playerMoveOption);
            if (!isSuccess)
            {
                string failMessage = "Invalid move option";
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move processing was denied because invalid parameter was passed");
                await SendMessageToCaller(failMessage);

                return GameEvents.ErrorOccured;
            }

            if (currentRound.Player(userId).SelectedOption.HasValue)
            {
                string failMessage = "You already made your move";
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move processing was denied because user already made his move");
                await SendMessageToCaller(failMessage);

                return GameEvents.ErrorOccured;
            }

            if (playerMoveOption == MoveOptions.Undefined)
            {
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: User aborted round");

                currentRound.CreateNewRound();
                await Clients.OthersInGroup(currentRoom.RoomToken).SendAsync(GameEvents.GameEnd, 
                                                                "Round is canceled, opponent's time to make move expired");
                
                await Clients.Caller.SendAsync(GameEvents.GameEnd, 
                    "Round is canceled, your time to make move expired\nPress 'Enter' to continue...");

                await Clients.OthersInGroup(currentRoom.RoomToken).SendAsync(GameEvents.GameAborted);
                
                return GameEvents.GameEnd;
            }
            currentRound.MakeMove(userId, playerMoveOption);

            if (currentRound.Opponent(userId).SelectedOption.HasValue && currentRound.Player(userId).SelectedOption.HasValue)
            {
                //var playerMoveOption = currentRound.Player(userId).SelectedOption.Value;
                var opponentMoveOption = currentRound.Opponent(userId).SelectedOption.Value;
            
                GameOutcome playerResult = _gameService.GetGameResult(playerMoveOption, opponentMoveOption);
                string playerSummary = $"Your choice: {playerMoveOption}\n" +
                                       $"Opponent's choice: {opponentMoveOption}\n" +
                                       $"Round result: {playerResult}\n";

                var isPlayersStatSaved = await _statisticsService.SaveAsync(currentRound.Player(userId).PlayerId, playerResult, playerMoveOption);
                if (!isPlayersStatSaved)
                {               
                    _logger.LogInformation($"{nameof(GameHub)} | {userId}: Round statistics wasn't saved");
                }
                
                await Clients.Caller.SendAsync(GameEvents.GameEnd, playerSummary);
                
                
                GameOutcome opponentResult = _gameService.GetGameResult(opponentMoveOption, playerMoveOption);
                string opponentSummary = $"Your choice: {opponentMoveOption}\n" +
                                         $"Opponent's choice: {playerMoveOption}\n" +
                                         $"Round result: {opponentResult}\n";
                
                var isOpponentStatSaved = await _statisticsService.SaveAsync(currentRound.Opponent(userId).PlayerId, opponentResult, opponentMoveOption);
                if (!isOpponentStatSaved)
                {
                    _logger.LogInformation($"{nameof(GameHub)} | {userId}: Round statistics wasn't saved");
                }
                
                await Clients.OthersInGroup(currentRoom.RoomToken).SendAsync(GameEvents.GameEnd, opponentSummary);
                currentRound.CreateNewRound();
                
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move successfully processed");
                return GameEvents.GameEnd;
            }
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Move successfully processed");
            await SendMessageToCaller("Waiting for opponent to make move");
            return GameEvents.WaitingForPlayerToPlay;
        }

        public async Task<string> LeaveGame()
        {
            //string userId = GetUserToken();
            string userId = Context.ConnectionId;
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Game disconnecting was requested");

            GameRoom playerRoom = _gameStorageService.GetRoomByPlayer(userId);

            if (playerRoom == null)
            {
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Already disconnected. User is not in the game right now");

                await SendMessageToCaller("You are not in the game right now");
                return GameEvents.ErrorOccured;
            }
            string player1Id = playerRoom.Series.Player1;
            string player2Id = playerRoom.Series.Player2;
            string roomToken = playerRoom.RoomToken;
            
            bool isSuccess = _gameStorageService.DeleteRoom(playerRoom.RoomToken);
            
            if (isSuccess)
            {
                await SendMessageToGroupExceptCaller(roomToken, "\nYour opponent left the game");
                await SendMessageToCaller("\nDisconnected");
                
                await Clients.Group(roomToken).SendAsync(GameEvents.GameClosed);

                await Groups.RemoveFromGroupAsync(player1Id, roomToken);
                if (!string.IsNullOrEmpty(player2Id))
                {
                    await Groups.RemoveFromGroupAsync(player2Id, roomToken);
                }
                
                _logger.LogInformation($"{nameof(GameHub)} | {userId}: Successfully disconnected.");

                return GameEvents.GameClosed;
            }
            
            _logger.LogInformation($"{nameof(GameHub)} | {userId}: Disconnection ended with error");

            return GameEvents.ErrorOccured;
        }

        private async Task SendMessageToCaller(string message)
        {
            await Clients.Caller.SendAsync(GameEvents.ReceiveMessage, $"\n{message}");
            _logger.LogInformation($"{nameof(GameHub)}: Message sent to the caller");
        }
        
        private async Task SendMessageToGroupExceptCaller(string groupName, string message)
        {
            await Clients.OthersInGroup(groupName).SendAsync(GameEvents.ReceiveMessage, $"\n{message}");
            _logger.LogInformation($"{nameof(GameHub)}: Message sent to the caller's group except himself");
        }
        
        private async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(GameEvents.ReceiveMessage, $"\n{message}");
            _logger.LogInformation($"{nameof(GameHub)}: Message sent to the group");
        }
    }
}
