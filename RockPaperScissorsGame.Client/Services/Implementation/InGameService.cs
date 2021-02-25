using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Client.Services.Abstract;
using RockPaperScissorsGame.Client.Settings;
using RockPaperScissorsGame.Common;

namespace RockPaperScissorsGame.Client.Services.Implementation
{
    public class InGameService : IInGameService
    {
        private bool _isWaiting;
        private bool _isConnectionConfigured;
        private readonly IConnectionService _connectionService;
        private readonly IOptions<AppSettings> _appSettings;

        public InGameService(IConnectionService connectionService, IOptions<AppSettings> appSettings)
        {
            _connectionService = connectionService;
            _appSettings = appSettings;
        }

        private async Task<bool> EnsureConnectionConfigured(string playerId)
        {
            if (!await _connectionService.EnsureConnectionAsync(playerId)) return false;
            if (_isConnectionConfigured) return true;
            
            _connectionService.Connection.On<string>(GameEvents.GameEnd, (result) =>
            {
                _isWaiting = false;
            });

            _isConnectionConfigured = true;
            return true;
        }
        public async Task MakeMoveAsync(string playerId, Figure figure, bool isMoveMadeInTime)
        {
            if (await EnsureConnectionConfigured(playerId))
            {
                if (!isMoveMadeInTime)
                {
                    await _connectionService.Connection.InvokeAsync("MakeMove", Figure.Undefined.ToString());
                    return;
                }
                
                string makeMoveResult = await _connectionService.Connection.InvokeAsync<string>("MakeMove", figure.ToString());
                if (makeMoveResult.Equals(GameEvents.WaitingForPlayerToPlay))
                {
                    _isWaiting = true;

                    int timeout = _appSettings.Value?.MoveTimeout ?? 20000;
                    int poolingTimeout = 2000;
                    
                    for (int i = 0; i < timeout / poolingTimeout; i++)
                    {
                        await Task.Delay(poolingTimeout);
                        if (_isWaiting == false)
                        {
                            break;
                        }
                    }
                }
                
                
            }
               
        }
        
        public async Task LeaveGameAsync(string playerId)
        {
            if (await EnsureConnectionConfigured(playerId))
            {
                await _connectionService.Connection.InvokeAsync("LeaveGame");
            }
        }
    }
}