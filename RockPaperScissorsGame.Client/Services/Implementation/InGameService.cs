using System;
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
        private readonly IOptions<TimeoutSettings> _timeoutSettings;

        public InGameService(IConnectionService connectionService, IOptions<TimeoutSettings> timeoutSettings)
        {
            _connectionService = connectionService;
            _timeoutSettings = timeoutSettings;
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
        public async Task MakeMoveAsync(string playerId, MoveOptions figure, bool isMoveMadeInTime)
        {
            if (await EnsureConnectionConfigured(playerId))
            {
                if (!isMoveMadeInTime)
                {
                    await _connectionService.Connection.InvokeAsync("MakeMove", MoveOptions.Undefined.ToString());
                    return;
                }
                
                string makeMoveResult = await _connectionService.Connection.InvokeAsync<string>("MakeMove", figure.ToString());
                if (makeMoveResult.Equals(GameEvents.WaitingForPlayerToPlay))
                {
                    _isWaiting = true;

                    int timeout = _timeoutSettings.Value?.MoveTimeout ?? 20000;
                    int poolingTimeout = 2000;
                    
                    for (int i = 0; i < timeout / poolingTimeout; i++)
                    {
                        await Task.Delay(poolingTimeout);
                        if (_isWaiting == false)
                        {
                            break;
                        }
                    }

                    if (_isWaiting)
                    {
                        Console.WriteLine("You can wait for an opponent or leave the game"); 
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