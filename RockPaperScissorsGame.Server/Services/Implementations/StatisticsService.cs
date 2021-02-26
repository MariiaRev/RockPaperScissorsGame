using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RockPaperScissorsGame.Common;
using RockPaperScissorsGame.Server.Helpers;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Models.Db;
using RockPaperScissorsGame.Server.Models.Game;
using RockPaperScissorsGame.Server.Options;
using RockPaperScissorsGame.Server.Services.Abstractions;

namespace RockPaperScissorsGame.Server.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStorage<UserStatistics> _statistics;
        private readonly IStorage<string> _tokens;
        private readonly JsonDataService<StatisticsDb> _jsonDataService;
        private readonly JsonPathsOptions _options;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IStorage<UserStatistics> statistics,
            IStorage<string> tokens,
            JsonDataService<StatisticsDb> jsonDataService,
            IOptions<JsonPathsOptions> jsonOptions,
            ILogger<StatisticsService> logger)
        {
            _statistics = statistics;
            _tokens = tokens;
            _jsonDataService = jsonDataService;
            _options = jsonOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Saving user statistics both in the storage and in the json file database
        /// if user is identified by his/her authorization <paramref name="token"/>.
        /// </summary>
        /// <param name="token">User's authorization token.</param>
        /// <param name="outcome">Game outcome. One of <see cref="GameOutcome"/></param>
        /// <param name="move">Chosen move option. One of <see cref="MoveOptions"/></param>
        /// <returns>Returns true if user was found by token, else returns false.</returns>
        public async Task<bool> SaveAsync(string token, GameOutcome outcome, MoveOptions move)
        {
            // get user id
            var userWithToken = (await _tokens.GetAllAsync()).Where(tk => tk.Item == token).FirstOrDefault();

            // if user was not found
            if (userWithToken == null)
            {
                // only one logger message here so as not to litter the file with messages
                // about saving statistics, since this method will be called very often
                _logger.LogInformation($"{nameof(StatisticsService)}: User was not identified for saving statistics. The authorization token did not exist or expired.");
                return false;
            }

            // get user id
            var userId = userWithToken.Id;

            // add user statistics to the storage if it doesn't exist
            await _statistics.AddAsync(new UserStatistics(userId), userId, new StatisticsEqualityComparer());

            // change state
            var record = await _statistics.GetAsync(userId);
            record.AddRoundInfo(DateTime.Now, outcome, move);

            // map with StatisticsDb
            var statisticsToSave = ModelsMapper.ToStatisticsDb(record);

            //save to file
            await _jsonDataService.WriteAsync(_options.StatisticsPath + $"{userId}.json", statisticsToSave);

            return true;
        }


        /// <summary>
        /// Setup statistics storage with data from the json file database.
        /// Intended use - at server start.
        /// </summary>
        /// <returns>Returns no value.</returns>
        public async void SetupStorage()
        {
            _logger.LogInformation($"{nameof(StatisticsService)}: Initial setup of the statistics storage.");
            var statisticsFiles = Directory.GetFiles(_options.StatisticsPath);
            StatisticsDb fileData;
            _logger.LogInformation($"{nameof(StatisticsService)}: {statisticsFiles.Length} files with statistics by each user were(was) found.");

            foreach (var file in statisticsFiles)
            {
                fileData = await _jsonDataService.ReadJsonObjectAsync(file);

                if (fileData != null)   //if file is not empty
                {
                    var userStatistics = ModelsMapper.ToUserStatistics(fileData);

                    await _statistics.AddAsync(userStatistics, fileData.UserId);
                }
            }

            _logger.LogInformation($"{nameof(StatisticsService)}: Statistics for {statisticsFiles.Length} users was added to the statistics storage.");
        }

        /// <summary>
        /// Saves user in-game time both in the storage and in the json file database if user is identified by his/her authorization  
        /// <paramref name="token"/> and <paramref name="gameTime"/> is successfully parsed to <see cref="long"/>.
        /// </summary>
        /// <param name="token">User's authorization token.</param>
        /// <param name="gameTime">User in-game time in ticks as <see cref="string"/> variable.</param>
        /// <returns>Returns null if user was found by token and in-game time was successfully 
        /// parsed to <see cref="long"/>, else returns message with error detalization.</returns>
        public async Task<string> SaveGameTime(string token, string gameTime)
        {
            // get user id
            var userWithToken = (await _tokens.GetAllAsync()).Where(tk => tk.Item == token).FirstOrDefault();

            // if user was not found
            if (userWithToken == null)
            {
                var message = "The authorization token did not exist or expired.";
                _logger.LogInformation($"{nameof(StatisticsService)}: User was not identified for saving in-game time. {message}");
                return message;
            }

            // get user id
            var userId = userWithToken.Id;
            _logger.LogInformation($"{nameof(StatisticsService)}: User with id {userId} was identified by his/her authorization token for saving in-game time.");

            // convert game time
            if (!long.TryParse(gameTime, out var gameTimeTicks))
            {
                var message = $"Cannot parse {gameTime} to {typeof(long)} for the user with id {userId}.";
                _logger.LogInformation($"{nameof(StatisticsService)}: {message}");
                return message;
            }

            var userStatistics = await _statistics.GetAsync(userId);

            // create empty statistics if it doesn't exist
            if (userStatistics == null)
            {
                userStatistics = new UserStatistics(userId);
            }

            // save to the storage
            userStatistics.AddGameTime(gameTimeTicks);
            await _statistics.AddOrUpdateAsync(userId, userStatistics);
            _logger.LogInformation($"{nameof(StatisticsService)}: In-game time was saved to the statistics storage for the user with id {userId}.");

            // map with StatisticsDb
            var statisticsToSave = ModelsMapper.ToStatisticsDb(userStatistics);

            //save to file
            var path = $"{_options.StatisticsPath}{userId}.json";
            await _jsonDataService.WriteAsync(path, statisticsToSave);
            _logger.LogInformation($"{nameof(StatisticsService)}: In-game time was saved to the file {path} for the user with id {userId}.");

            return null;
        }
    }
}
