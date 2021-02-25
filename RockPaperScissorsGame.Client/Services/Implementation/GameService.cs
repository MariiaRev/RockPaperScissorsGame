using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class GameService : IGameService
    {
        private bool _isConnectionConfigured;
        private readonly IConnectionService _connectionService;
        
        public GameService(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        private async Task<bool> EnsureConnectionConfigurationAsync(string playerId)
        {
            if (!await _connectionService.EnsureConnectionAsync(playerId)) return false;

            if (_isConnectionConfigured) return true;
            
            _isConnectionConfigured = true;
            return true;
        }
        
        public async Task<string> CreatePrivateRoomAsync(string playerId)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("CreatePrivateRoom");
                return resultEvent;
            }
            return GameEvents.ErrorOccured;
        }
        
        public async Task<string> JoinPrivateRoom(string playerId, string roomToken)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("JoinPrivateRoom", roomToken);
                return resultEvent;
            }
            
            return GameEvents.ErrorOccured;
        }

        public async Task<string> FindPublicGame(string playerId)
        {
            if (await EnsureConnectionConfigurationAsync(playerId))
            {
                string resultEvent = await _connectionService.Connection.InvokeAsync<string>("FindPublicRoom");
                return resultEvent;
            }

            return GameEvents.ErrorOccured;
        }
    }
}