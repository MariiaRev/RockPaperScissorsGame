using System.Threading.Tasks;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IUsersService
    {
        Task<bool> SaveAsync(string login, string password);
        void SetupStorage();
    }
}
