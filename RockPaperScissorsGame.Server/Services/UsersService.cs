using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Server.Options;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Helpers;
using RockPaperScissorsGame.Server.Models.Db;

namespace RockPaperScissorsGame.Server.Services
{
    public class UsersService : IUsersService
    {
        private readonly IStorage<User> _users;
        private readonly JsonDataService<UserDb> _userDataService;
        private readonly IStorage<UserStatistics> _statistics;
        private readonly JsonDataService<StatisticsDb> _statDataService;
        private readonly JsonPathsOptions _options;

        public UsersService(
            IStorage<User> users,
            IStorage<UserStatistics> statistics,
            JsonDataService<UserDb> userDataService,
            JsonDataService<StatisticsDb> statDataService,
            IOptions<JsonPathsOptions> options)
        {
            _users = users;
            _statistics = statistics;
            _userDataService = userDataService;
            _statDataService = statDataService;
            _options = options.Value;
        }

        /// <summary>
        /// Saving user both in the storage and in the json file database.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="password">User password.</param>
        /// <returns>Returns whether the user was saved.
        /// True if user was saved.
        /// False if user was not saved (because already exists).
        /// </returns>
        public async Task<bool> SaveAsync(string login, string password)
        {
            // add user to the user storage
            var userId = await _users.AddAsync(new User(login, password), null, new UserEqualityComparer());

            // if user was not added because already exists
            if (userId == null)
            {
                return false;
            }

            // create empty statistics for the new user in memory storage
            var userStatistics = new UserStatistics((int)userId);
            await _statistics.AddAsync(userStatistics, (int)userId);

            // save empty statistics for the new user to the json-file database
            var statisticsToSave = ModelsMapper.ToStatisticsDb(userStatistics);
            await _statDataService.WriteAsync($"{_options.StatisticsPath}{userId}.json", statisticsToSave);

            // get all users
            var users = await _users.GetAllAsync();

            // map with UserDb
            var usersToSave = users.Select(user => ModelsMapper.ToUserDb(user.Id, user.Item));

            // save to file
            await _userDataService.WriteAsync(_options.UsersPath, usersToSave);
                
            return true;
        }

        /// <summary>
        /// Setup user storage with data from the json file database.
        /// Intended use - at server start.
        /// </summary>
        /// <returns>Returns no value.</returns>
        public async void SetupStorage()
        {
            var data = await _userDataService.ReadJsonArrayAsync(_options.UsersPath);

            foreach (var user in data)
            {
                await _users.AddAsync(ModelsMapper.ToUser(user), user.UserId);
            }
        }
    }
}
