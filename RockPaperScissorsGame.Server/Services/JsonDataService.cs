using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Services
{
    public class JsonDataService<T> where T: class
    {
        /// <summary>
        /// Asynchronous reading data array from a json file.
        /// </summary>
        /// <param name="path">File path for reading data.</param>
        /// <returns>Returns a list of T objects.</returns>
        public async Task<IEnumerable<T>> ReadJsonArrayAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<List<T>>(json);
            
            return data;
        }

        /// <summary>
        /// Asynchronous reading data object from a json file.
        /// </summary>
        /// <param name="path">File path for reading data object.</param>
        /// <returns>Returns T object.</returns>
        public async Task<T> ReadJsonObjectAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<T>(json);

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

            await File.WriteAllTextAsync(path, json);
        }
    }
}
