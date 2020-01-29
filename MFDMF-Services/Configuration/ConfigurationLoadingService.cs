using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public List<IModuleDefinition> LoadModulesConfigurationFile(string jsonFile, List<IDisplayDefinition> displays)
        {
            var modulesList = new List<IModuleDefinition>();
            try
            {
                if (!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException($"Unable to find the file {jsonFile}");
                }
                _logger?.LogInformation($"Loading configuration from {jsonFile}");
                var fileContent = File.ReadAllText(jsonFile);
                var moduleDefinitions = JsonConvert.DeserializeObject<List<ModuleDefinition>>(fileContent);
                PreProcessModules(ref moduleDefinitions, displays);
                modulesList.AddRange(moduleDefinitions);
                return modulesList;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonFile} {ex}");
                throw;
            }
        }

		#endregion Public methods

		#region Private helpers

		/// <summary>
		/// Make sure the Hierarchy is setup
		/// </summary>
		/// <param name="modules"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void PreProcessModules(ref List<ModuleDefinition> modules, List<IDisplayDefinition> displays)
        {
            modules?.ForEach(arg =>
            {
                if (arg != null)
                {
                    arg.Enabled ??= true; 
                    arg.FilePath ??= _settings.FilePath;

                    _logger.LogDebug($"Module: {arg.ModuleName} Display Name: {arg.DisplayName} processing...");

                    arg?.Configurations?.ForEach(config =>
                    {
                        config.Enabled ??= arg.Enabled ?? true;
                        config.ModuleName ??= arg?.ModuleName;
                        config.FilePath ??= arg?.FilePath;
                        config.FileName ??= arg?.FileName;
                        config.UseAsSwitch ??= false;
                        config.MakeOpaque ??= false;
                        config.Center ??= false;
                        config.Opacity ??= 1.0F;

                        // Get the Display for the configuration
                        var currentDisplay = displays?.FirstOrDefault(dd => dd.Name == config?.Name || (config?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
                        // Get the width and height of the logical display
                        config.Width ??= currentDisplay?.ImageGeometry?.Width ?? currentDisplay?.Width ?? 0;
                        config.Height ??= currentDisplay?.ImageGeometry?.Height ?? currentDisplay?.Height ?? 0;
                        // Physical origin point of the logical display
                        var origin = new System.Drawing.Point((currentDisplay?.Left ?? 0), (currentDisplay?.Top ?? 0));
                        if (config.Center ?? false)
                        {
                            // Get the center of the display for this configuration
                            origin = config.GetCenterTo(currentDisplay);
                        }
                        // Offset as requested by configs Left and Top coordinates
                        // TODO: Determine behavior of the Left and Top coordinates in relation to the currentDisplay
                        config.Left = origin.X + (currentDisplay?.ImageGeometry?.Left ?? config?.Left ?? 0);
                        config.Top = origin.Y + (currentDisplay?.ImageGeometry?.Top ?? config?.Top ?? 0);
                        /*
                        config.Left = origin.X + (currentDisplay?.ImageGeometry?.Left ?? currentDisplay?.Left ?? config?.Left ?? 0);
                        config.Top = origin.Y + (currentDisplay?.ImageGeometry?.Top ?? currentDisplay?.Top ?? config?.Top ?? 0);
                        */
                        config.Opacity ??= currentDisplay?.Opacity;
                        config.XOffsetStart ??= currentDisplay?.XOffsetStart  ?? 0;
                        config.XOffsetFinish ??= currentDisplay?.XOffsetFinish ?? 0;
                        config.YOffsetStart ??= currentDisplay?.YOffsetStart ?? 0;
                        config.YOffsetFinish ??= currentDisplay?.YOffsetFinish ?? 0;

                        _logger.LogDebug($"Configuration {config.Name} using {currentDisplay?.Name ?? "Scratch"} Using file: {config?.FileName ?? "None"} Size: ({config.Width ?? 0},{config.Height ?? 0}) Location: ({config.Left ?? 0}, {config.Top ?? 0})");
                        _logger.LogDebug($"Cropping offsets ({config.XOffsetStart??0}, {config.YOffsetStart??0}) to ({config.XOffsetFinish??0}, {config.YOffsetFinish??0}) Opacity: {config.Opacity}");

                        ConfigurationDefinition.WalkConfigurattionDefinitionsWithAction(config, (subConfig) =>
                        {
                            var currentSubDisplay = displays?.FirstOrDefault(dd => dd.Name == subConfig?.Name || (subConfig?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false)) ?? currentDisplay;
                            subConfig.Parent = config;
                            subConfig.ModuleName ??= config?.ModuleName;
                            subConfig.FilePath ??= config?.FilePath;
                            subConfig.FileName ??= config?.FileName;

                            subConfig.UseAsSwitch ??= config?.UseAsSwitch;
                            subConfig.Enabled ??= config?.Enabled;
                            subConfig.MakeOpaque ??= config?.MakeOpaque;

                            var origin = new System.Drawing.Point(0, 0);
                            // Center can originate from the parent configuration
                            subConfig.Center ??= config?.Center ?? false;
                            // The Width and Height can originate from the sub configurations display image geometry or the sub configuration itself
                            subConfig.Width ??= currentSubDisplay?.ImageGeometry?.Width ?? config.Width ?? 0;
                            subConfig.Height ??= currentSubDisplay?.ImageGeometry?.Height ?? config.Height ?? 0;
                            if (subConfig.Center ?? false)
                            {
                                origin = subConfig.GetCenterTo(config);
                            }
                            // Left and Top are used as offsets from the calculated origin 
                            // The offsets are the addition of the valuse from the sub configurations display image geometry and the sub configuration itself
                            // Example: Origin = (0,0) (default) subConfig.Left == 10 currentSubDisplay.ImageGeometry.Left == -10 then Left = 0
                            subConfig.Left = origin.X + (subConfig?.Left ?? 0) + (currentSubDisplay?.ImageGeometry?.Left  ?? 0);
                            subConfig.Top = origin.Y + (subConfig?.Top ?? 0) + (currentSubDisplay?.ImageGeometry?.Top ?? 0);

                            // The cropping offsets can originate from the sub configurations display offset geometry and from their parent configurations
                            subConfig.XOffsetStart ??= currentSubDisplay?.XOffsetStart ?? config?.XOffsetStart ?? 0;
                            subConfig.XOffsetFinish ??= currentSubDisplay?.XOffsetFinish ?? config.XOffsetFinish ?? 0;
                            subConfig.YOffsetStart ??= currentSubDisplay?.YOffsetStart ?? config.YOffsetStart ?? 0;
                            subConfig.YOffsetFinish ??= currentSubDisplay?.YOffsetFinish ?? config.YOffsetFinish ?? 0;
                            subConfig.Opacity ??= currentSubDisplay?.Opacity ?? config.Opacity;

                            _logger.LogDebug($"Configuration: {subConfig.Name} using {currentSubDisplay?.Name ?? "Scratch"} Using file: {subConfig?.FileName ?? "None"} Size: ({subConfig.Width ?? 0},{subConfig.Height ?? 0}) Location: ({subConfig.Left ?? 0}, {subConfig.Top ?? 0})");
                            _logger.LogDebug($"Cropping offsets: ({subConfig.XOffsetStart ?? 0}, {subConfig.YOffsetStart ?? 0}) to ({subConfig.XOffsetFinish ?? 0}, {subConfig.YOffsetFinish ?? 0}) Opacity: {subConfig.Opacity}");
                            _logger.LogDebug($"Image properties: UseAsSwitch: {subConfig.UseAsSwitch ?? false} MakeOpaque: {subConfig.MakeOpaque ?? false} Enabled: {subConfig.Enabled ?? false}");
                        });
                    });
                }
            });
        }
    }
    #endregion Private helpers
}
