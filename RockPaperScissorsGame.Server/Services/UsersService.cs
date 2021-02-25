using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UsersService> _logger;

        public UsersService(
            IStorage<User> users,
            IStorage<UserStatistics> statistics,
            JsonDataService<UserDb> userDataService,
            JsonDataService<StatisticsDb> statDataService,
            IOptions<JsonPathsOptions> options,
            ILogger<UsersService> logger)
        {
            _users = users;
            _statistics = statistics;
            _userDataService = userDataService;
            _statDataService = statDataService;
            _options = options.Value;
            _logger = logger;
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
            _logger.LogInformation($"{nameof(UsersService)}: Saving a new user.");

            // add user to the user storage
            var userId = await _users.AddAsync(new User(login, password), null, new UserEqualityComparer());

            // if user was not added because already exists
            if (userId == null)
            {
                _logger.LogInformation($"{nameof(UsersService)}: User was not added because the login {login} already exists.");
                return false;
            }

            _logger.LogInformation($"{nameof(UsersService)}: New user with login {login} was added to the storage.");

            // create empty statistics for the new user in memory storage
            var userStatistics = new UserStatistics((int)userId);
            await _statistics.AddAsync(userStatistics, (int)userId);
            _logger.LogInformation($"{nameof(UsersService)}: Empty statistics for the new user with login {login} was added to the storage.");

            // save empty statistics for the new user to the json-file database
            var statisticsToSave = ModelsMapper.ToStatisticsDb(userStatistics);
            await _statDataService.WriteAsync($"{_options.StatisticsPath}{userId}.json", statisticsToSave);
            _logger.LogInformation($"{nameof(UsersService)}: Empty statistics for the new user with login {login} was saved to the file {_options.StatisticsPath}{userId}.json.");

            // get all users
            var users = await _users.GetAllAsync();

            // map with UserDb
            var usersToSave = users.Select(user => ModelsMapper.ToUserDb(user.Id, user.Item));

            // save to file
            await _userDataService.WriteAsync(_options.UsersPath, usersToSave);
            _logger.LogInformation($"{nameof(UsersService)}: All users were rewritten in {_options.UsersPath}.");
                
            return true;
        }

        /// <summary>
        /// Setup user storage with data from the json file database.
        /// Intended use - at server start.
        /// </summary>
        /// <returns>Returns no value.</returns>
        public async void SetupStorage()
        {
            _logger.LogInformation($"{nameof(UsersService)}: Initial setup of the users storage.");
            var data = await _userDataService.ReadJsonArrayAsync(_options.UsersPath);

            foreach (var user in data)
            {
                await _users.AddAsync(ModelsMapper.ToUser(user), user.UserId);
            }

            _logger.LogInformation($"{nameof(UsersService)}: {await _users.CountAsync()} users were added to the users storage.");
        }
    }
}
