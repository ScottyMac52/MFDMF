using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MFDMF_Services.Configuration
{
    /// <summary>
    /// Servvice that is used to load Module configurations
    /// </summary>
    public class ConfigurationLoadingService : IConfigurationLoadingService
    {
		#region Private readonly fields

		private readonly AppSettings _settings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<IConfigurationLoadingService> _logger;

		#endregion Private readonly fields

		#region Ctor

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

        #endregion Ctor

        #region Public methods

        /// <summary>
        /// Loads a modules configuration file
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

        #endregion Public methods

    }
}
