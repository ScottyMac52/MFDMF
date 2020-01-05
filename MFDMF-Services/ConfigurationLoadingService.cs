using MFDMF_Models;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;

namespace MFDMF_Services
{
    public class ConfigurationLoadingService : IConfigurationLoadingService
    {
        private readonly AppSettings _settings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ConfigurationLoadingService> _logger;

        /// <summary>
        /// Constructor uses IoC dependency injection
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="loggerFactory"></param>
        public ConfigurationLoadingService(IOptions<AppSettings> settings, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger<ConfigurationLoadingService>();
            _settings = settings.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMFDMFDefinition LoadConfiguration()
        {
            return LoadConfigturation(Path.Combine(Directory.GetCurrentDirectory(), _settings.ConfigurationFile));
        }

        /// <summary>
        /// Loads the Configuration from the specified path
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        private IMFDMFDefinition LoadConfigturation(string jsonFile)
        {
            _logger?.LogInformation($"Loading configuration from {jsonFile}");
            var fileContent = File.ReadAllText(jsonFile);
            return MFDMFConfiguration.FromJson(_loggerFactory, fileContent);
        }

    }
}
