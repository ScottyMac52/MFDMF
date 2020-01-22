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
                if (!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException($"Unable to find the file {jsonFile}");
                }
                _logger?.LogInformation($"Loading configuration from {jsonFile}");
                var fileContent = File.ReadAllText(jsonFile);
                var moduleDefinitions = JsonConvert.DeserializeObject<List<ModuleDefinition>>(fileContent);
                return PreProcessModules(moduleDefinitions);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonFile} {ex}");
                throw;
            }
        }

        #endregion Public methods

        /// <summary>
        /// Make sure the Hierarchy is setup
        /// </summary>
        /// <param name="modules"></param>
        /// <returns></returns>
        private List<ModuleDefinition> PreProcessModules(List<ModuleDefinition> modules)
        {
            modules?.ForEach(arg =>
            {
                if (arg != null)
                {
                    arg.Enabled ??= true;
                    arg.FilePath ??= _settings.FilePath;

                    arg?.Configurations?.ForEach(config =>
                    {
                        config.ModuleName ??= arg.ModuleName;
                        config.FilePath ??= arg?.FilePath;
                        config.FileName ??= arg.FileName;
                        config.Enabled ??= arg.Enabled ??= true;

                        var currentConfig = config;
                        while ((currentConfig?.SubConfigurations?.Count ?? 0) > 0)
                        {
                            currentConfig?.SubConfigurations?.ForEach(subConfig =>
                            {
                                subConfig.Parent = currentConfig;
                                subConfig.ModuleName ??= config?.ModuleName;
                                subConfig.FilePath ??= config?.FilePath;
                                subConfig.FileName ??= config?.FileName;
                                subConfig.Enabled ??= config?.Enabled;
                                subConfig.Opacity ??= config.Opacity;
                                currentConfig = subConfig;
                            });
                        }
                    });
                }
            });


            return modules;
        }
    }
}
