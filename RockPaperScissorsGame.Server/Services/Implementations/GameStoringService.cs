using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using RockPaperScissorsGame.Server.Models.Game;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Services.Implementations
{
    public class GameStoringService : IGameStoringService
    {
        private readonly ILogger<GameStoringService> _logger;
        private readonly ConcurrentDictionary<string, GameRoom> _activeRooms;

        public GameStoringService(ILogger<GameStoringService> logger)
        {
            _logger = logger;
            _activeRooms = new ConcurrentDictionary<string, GameRoom>();
        }
        
        public bool CreateRoom(string token, bool isPrivate, string player1Id)
        {
            if (_activeRooms.ContainsKey(token))
            {
                _logger.LogInformation($"{nameof(GameStoringService)} Room with the same token already exists");
                return false;
            }

            bool isSuccess = _activeRooms.TryAdd(token, new GameRoom(token, player1Id, isPrivate));
            if (isSuccess)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} New room created");
            }
            else
            {
                _logger.LogInformation($"{nameof(GameStoringService)} Room wasn't created");
            }
            return isSuccess;
        }
        
        public bool JoinRoom(string roomToken, string playerId)
        {
            bool isRoomExists = _activeRooms.TryGetValue(roomToken, out GameRoom room);
            if (!isRoomExists)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} Requested room to join doesn't exist");
                return false;
            }

            if (room.Series.Player2 != null && room.Series.Player1 != null)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} Requested room to join full already");
                return false;
            }
            
            room.Series.Player2 = playerId;
            room.Series.CreateNewRound();
            return true;
        }
        
        public bool IsPrivateRoomExists(string roomToken)
        {
            bool isRoomExists = _activeRooms.TryGetValue(roomToken, out GameRoom room);

            return isRoomExists && room.IsPrivate;
        }

        public string GenerateRoomToken()
        {
            _logger.LogInformation($"{nameof(GameStoringService)} New room token generated");
            return Guid.NewGuid().ToString();
        }

        public string FindPublicRoom()
        {
            if (_activeRooms.IsEmpty)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} No actives rooms");
                return null;
            }
            
            GameRoom suitableRoom =  _activeRooms.Values
                .FirstOrDefault(r => !r.IsPrivate && string.IsNullOrWhiteSpace(r.Series.Player2));

            return suitableRoom?.RoomToken;
        }

        public GameRoom GetRoomByPlayer(string playerId)
        {
            if (_activeRooms.IsEmpty)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} No actives rooms");
                return null;
            }

            var room = _activeRooms.Values.FirstOrDefault(
                r => playerId.Equals(r.Series.Player1) || playerId.Equals(r.Series.Player2)
            );

            return room;
        }
        
        public bool DeleteRoom(string roomToken)
        {
            try
            {
                bool removeResult = _activeRooms.TryRemove(roomToken, out GameRoom _);
                return removeResult;
            }
            catch (ArgumentNullException)
            {
                _logger.LogInformation($"{nameof(GameStoringService)} Empty room token was provided");
                return false;
            }
        }
        
    }
}
