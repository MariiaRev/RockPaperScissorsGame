using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace RockPaperScissorsGame.Client.Services.Abstract
{
    public interface IConnectionService
    {
        HubConnection Connection { get; }
        Task<bool> EnsureConnectionAsync(string playerId);
    }
}