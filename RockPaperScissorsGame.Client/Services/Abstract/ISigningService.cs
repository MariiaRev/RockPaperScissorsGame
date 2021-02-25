using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Services.Abstract
{
    public interface ISigningService
    {
        Task<string> AuthorizeAsync(string login, string password);
        Task<(bool, string)> RegisterAsync(string login, string password);
    }
}