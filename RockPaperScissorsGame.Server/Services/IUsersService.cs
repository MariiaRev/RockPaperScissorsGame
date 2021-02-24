using System.Threading.Tasks;

namespace RockPaperScissorsGame.Server.Services
{
    public interface IUsersService
    {
        Task<bool> SaveAsync(string login, string password);
        void SetupStorage();
    }
}
