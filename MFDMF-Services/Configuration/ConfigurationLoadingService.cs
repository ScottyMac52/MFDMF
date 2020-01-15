using MFDMF_Models;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MFDMF_Services.Configuration
{
    public class ConfigurationLoadingService : IConfigurationLoadingService
    {
        private readonly AppSettings _settings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<IConfigurationLoadingService> _logger;

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
        /// Loads the current configuration
        /// </summary>
        /// <returns></returns>
        public IMFDMFDefinition LoadConfiguration()
        {
            return LoadConfigturation(Path.Combine(Directory.GetCurrentDirectory(), _settings.ConfigurationFile));
        }

        public ModuleDefinition LoadModuleConfigurationFile(string jsonFile)
        {
            try
            {
                if (!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException($"Unable to find the file {jsonFile}");
                }
                _logger?.LogInformation($"Loading configuration from {jsonFile}");
                var fileContent = File.ReadAllText(jsonFile);
                var moduleDefinition = JsonConvert.DeserializeObject<ModuleDefinition>(fileContent);
                return moduleDefinition;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonFile} {ex}");
                throw;
            }
        }

        /// <summary>
        /// Loads a module configuration file
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public List<ModuleDefinition> LoadModulesConfigurationFile(string jsonFile)
        {
            try
            {
                if(!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException($"Unable to find the file {jsonFile}");
                }
                _logger?.LogInformation($"Loading configuration from {jsonFile}");
                var fileContent = File.ReadAllText(jsonFile);
                var moduleDefinitions = JsonConvert.DeserializeObject<List<ModuleDefinition>>(fileContent);
                return moduleDefinitions;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonFile} {ex}");
                throw;
            }
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
            return MFDMFDefinition.FromJson(_loggerFactory, fileContent);
        }

    }
}
