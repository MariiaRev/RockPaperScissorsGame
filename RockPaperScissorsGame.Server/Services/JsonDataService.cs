using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RockPaperScissorsGame.Server.Services
{
    public class JsonDataService<T> where T: class
    {
        private readonly ILogger<JsonDataService<T>> _logger;
        
        public JsonDataService(ILogger<JsonDataService<T>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Asynchronous reading data array from a json file.
        /// </summary>
        /// <param name="path">File path for reading data.</param>
        /// <returns>Returns a list of T objects.</returns>
        public async Task<IEnumerable<T>> ReadJsonArrayAsync(string path)
        {
            _logger.LogInformation($"Reading data array from {path}.");
            var json = await File.ReadAllTextAsync(path);
            _logger.LogInformation($"File {path} was read.");

            var data = JsonConvert.DeserializeObject<List<T>>(json);
            _logger.LogInformation($"Data array from the file {path} was deserialized.");
            
            return data;
        }

        /// <summary>
        /// Asynchronous reading data object from a json file.
        /// </summary>
        /// <param name="path">File path for reading data object.</param>
        /// <returns>Returns T object.</returns>
        public async Task<T> ReadJsonObjectAsync(string path)
        {
            _logger.LogInformation($"Reading data object from {path}.");
            var json = await File.ReadAllTextAsync(path);
            _logger.LogInformation($"File {path} was read.");

            var data = JsonConvert.DeserializeObject<T>(json);
            _logger.LogInformation($"Data object from the file {path} was deserialized.");

            return data;
        }

        /// <summary>
        /// Asynchronous writing data to a json file.
        /// </summary>
        /// <param name="path">File path where to write <see cref="data"/></param>
        /// <param name="data">Data to write</param>
        /// <returns>Returns no value.</returns>
        public async Task WriteAsync(string path, object data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            });

            _logger.LogInformation($"Writing data to the file {path}.");
            await File.WriteAllTextAsync(path, json);
            _logger.LogInformation($"Data was written to the file {path}.");
        }
    }
}
