using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MFDMF_Services.Configuration
{
    public class ModelLoaderService : IModelLoaderService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ModelLoaderService> _logger;

        public ModelLoaderService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ModelLoaderService>();
        }

        /// <summary>
        /// Attempts to load type T from the specified file
        /// </summary>
        /// <typeparam name="T">The type to Deserialize</typeparam>
        /// <param name="jsonFile">File to read from</param>
        /// <returns></returns>
        public async Task<T> LoadJsonFileAsync<T>(string jsonFile)
        {
            try
            {
                if (!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException($"Unable to find the file {jsonFile}");
                }
                _logger?.LogInformation($"Loading {typeof(T).Name} from {jsonFile}");
                var fileContent = await File.ReadAllTextAsync(jsonFile);
                var moduleDefinition = JsonConvert.DeserializeObject<T>(fileContent);
                return moduleDefinition;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonFile} {ex}");
                throw;
            }
        }
    }
}
