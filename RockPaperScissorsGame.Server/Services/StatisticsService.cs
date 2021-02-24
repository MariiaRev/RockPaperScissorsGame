using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RockPaperScissorsGame.Server.Models;
using RockPaperScissorsGame.Server.Helpers;
using RockPaperScissorsGame.Server.Options;
using RockPaperScissorsGame.Server.Models.Db;

namespace RockPaperScissorsGame.Server.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStorage<UserStatistics> _statistics;
        private readonly IStorage<string> _tokens;
        private readonly JsonDataService<StatisticsDb> _jsonDataService;
        private readonly JsonPathsOptions _options;

        public StatisticsService(
            IStorage<UserStatistics> statistics,
            IStorage<string> tokens,
            JsonDataService<StatisticsDb> jsonDataService,
            IOptions<JsonPathsOptions> jsonOptions)
        {
            _statistics = statistics;
            _tokens = tokens;
            _jsonDataService = jsonDataService;
            _options = jsonOptions.Value;
        }

        /// <summary>
        /// Saving user statistics both in the storage and in the json file database.
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
                return false;
            }

            // get user id
            var userId = userWithToken.Id;

            // add user to the storage if (s)he doesn't exist
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
            var statisticsFiles = Directory.GetFiles(_options.StatisticsPath);
            StatisticsDb fileData;

            foreach (var file in statisticsFiles)
            {
                fileData = await _jsonDataService.ReadJsonObjectAsync(file);

                if (fileData != null)   //if file is not empty
                {
                    var userStatistics = ModelsMapper.ToUserStatistics(fileData);

                    await _statistics.AddAsync(userStatistics, fileData.UserId);
                }
            }
        }
    }
}
