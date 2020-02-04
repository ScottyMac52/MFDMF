using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                        // Get the Display for the configuration and get the placement
                        var currentDisplay = displays?.FirstOrDefault(dd => dd.Name == config?.Name || (config?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
                        if (currentDisplay == null)
                        {
                            var errorMessage = $"THe configuration: {config.Name} in Module: {config.ModuleName} doesn't have a related logical display!";
                            _logger.LogError(errorMessage);
                            throw new ArgumentOutOfRangeException(errorMessage);
                        }

                        config.Logger = _logger;
                        config.Enabled ??= arg.Enabled ?? true;
                        config.ModuleName ??= arg?.ModuleName;
                        config.FilePath ??= arg?.FilePath;
                        config.FileName ??= arg?.FileName;
                        config.UseAsSwitch ??= false;
                        config.MakeOpaque ??= false;
                        config.Center ??= false;
                        config.Opacity ??= 1.0F;
                        config.RulerName = (_settings.ShowRulers ?? false) ? $"Ruler-{_settings.RulerSize ?? 0}" : null;
        
                        var placement = GetPlacementRect(config, currentDisplay);
                        config.Left = placement.X;
                        config.Top = placement.Y;
                        config.Width = placement.Width;
                        config.Height = placement.Height;

                        config.Opacity ??= currentDisplay?.Opacity;
                        var croppingRect = GetCroppingRect(config, currentDisplay);
                        config.XOffsetStart = croppingRect.Left;
                        config.XOffsetFinish ??= croppingRect.Right;
                        config.YOffsetStart ??= croppingRect.Top;
                        config.YOffsetFinish ??= croppingRect.Bottom;

                        var throttleType = (_settings?.UseCougar ?? false) ? "HC" : "WH";
                        config.ThrottleType = throttleType;

                        _logger.LogDebug($"Configuration {config.Name} using {currentDisplay?.Name ?? "Scratch"} Using file: {config?.FileName ?? "None"} Size: ({config.Width ?? 0},{config.Height ?? 0}) Location: ({config.Left ?? 0}, {config.Top ?? 0})");
                        _logger.LogDebug($"Cropping offsets ({config.XOffsetStart??0}, {config.YOffsetStart??0}) to ({config.XOffsetFinish??0}, {config.YOffsetFinish??0}) Opacity: {config.Opacity}");

                        ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(config, (subConfig) =>
                        {
                            var currentSubDisplay = displays?.FirstOrDefault(dd => dd.Name == subConfig?.Name || (subConfig?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
                            subConfig.Logger = _logger;
                            subConfig.Parent = config;
                            subConfig.ModuleName ??= config?.ModuleName;
                            subConfig.FilePath ??= config?.FilePath;
                            subConfig.FileName ??= config?.FileName;
                            subConfig.UseAsSwitch ??= config?.UseAsSwitch;
                            subConfig.Enabled ??= config?.Enabled;
                            subConfig.MakeOpaque ??= config?.MakeOpaque;
                            subConfig.ThrottleType = throttleType;
                            subConfig.RulerName = (_settings.ShowRulers ?? false) ? $"Ruler-{_settings.RulerSize ?? 0}" : "";

                            var placement = GetPlacementRect(config, currentSubDisplay, subConfig);
                            subConfig.Left = placement.X;
                            subConfig.Top = placement.Y;
                            subConfig.Width = placement.Width;
                            subConfig.Height = placement.Height;

                            // The cropping offsets can originate from the sub configurations display offset geometry and from their parent configurations
                            var croppingRect = GetCroppingRect(config, currentDisplay, subConfig);
                            subConfig.XOffsetStart ??= croppingRect.Left;
                            subConfig.XOffsetFinish ??= croppingRect.Right;
                            subConfig.YOffsetStart ??= croppingRect.Top;
                            subConfig.YOffsetFinish ??= croppingRect.Bottom;
                            subConfig.Opacity ??= currentSubDisplay?.Opacity ?? config.Opacity;

                            _logger.LogDebug($"Configuration: {subConfig.Name} using {currentSubDisplay?.Name ?? "Scratch"} Using file: {subConfig?.FileName ?? "None"} Size: ({subConfig.Width ?? 0},{subConfig.Height ?? 0}) Location: ({subConfig.Left ?? 0}, {subConfig.Top ?? 0})");
                            _logger.LogDebug($"Cropping offsets: ({subConfig.XOffsetStart ?? 0}, {subConfig.YOffsetStart ?? 0}) to ({subConfig.XOffsetFinish ?? 0}, {subConfig.YOffsetFinish ?? 0}) Opacity: {subConfig.Opacity}");
                            _logger.LogDebug($"Image properties: UseAsSwitch: {subConfig.UseAsSwitch ?? false} MakeOpaque: {subConfig.MakeOpaque ?? false} Enabled: {subConfig.Enabled ?? false}");
                        });
                    });
                }
            });
        }

        private static Rectangle GetCroppingRect(IOffsetGeometry config, IDisplayDefinition display, IOffsetGeometry secondary = null)
        {
            var xStart = secondary?.XOffsetStart ?? config?.XOffsetStart ?? display?.XOffsetStart ?? 0;
            var xFinish = secondary?.XOffsetFinish ?? config?.XOffsetFinish ?? display?.XOffsetFinish ?? 0;
            var yStart = secondary?.YOffsetStart ?? config?.YOffsetStart ?? display?.YOffsetStart ?? 0;
            var yFinish = secondary?.YOffsetFinish ?? config?.YOffsetFinish ?? display?.YOffsetFinish ?? 0;
            return new Rectangle(new Point(xStart, yStart), new Size(xFinish - xStart, yFinish - yStart));
        }

        private static Rectangle GetPlacementRect(IDisplayGeometry config, IDisplayDefinition display, IDisplayGeometry subConfig = null)
        {
            // Get the size
            var size = GetSize(config, display, subConfig);

            if(subConfig == null)
            {
                config.Width = size.Width;
                config.Height = size.Height;
            }
            else
            {
                subConfig.Width = size.Width;
                subConfig.Height = size.Height;
            }

            // get the location using the new width and height
            var location = GetLocation(config, display, subConfig);
            return new Rectangle(location, size);
        }

        /// <summary>
        /// Gets the location
        /// </summary>
        /// <param name="config"></param>
        /// <param name="display"></param>
        /// <param name="subConfig"></param>
        /// <returns></returns>
        private static Point GetLocation(IDisplayGeometry config, IDisplayDefinition display = null, IDisplayGeometry subConfig = null)
        {
            IDisplayGeometry child, parent;
            int left, top;
            if (subConfig != null)
            {
                child = subConfig;
                parent = config;
            }
            else
            {
                child = config;
                parent = display;
            }

            var origin = new Point(child?.Left ?? 0, child?.Top ?? 0);
            if (child.Center ?? false)
            {
                origin = child.GetCenterTo(parent);
            }

            child.Left = origin.X;
            child.Top = origin.Y;

            if (subConfig == null)
            {
                left = (parent?.Left ?? 0) + (child?.Left ?? 0);
                top = (parent.Top ?? 0) + (child?.Top ?? 0);
            }
            else
            {
                left = child?.Left ?? 0;
                top = child?.Top ?? 0;
            }

            // Offset as requested by configs Left and Top coordinates
            left += (display?.ImageGeometry?.Left ?? 0);
            top +=  (display?.ImageGeometry?.Top ?? 0);
            return new Point(left, top);
        }

        /// <summary>
        /// Get the size of the container 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="display"></param>
        /// <param name="subConfig"></param>
        /// <returns></returns>
        private static Size GetSize(IDisplayGeometry config, IDisplayDefinition display, IDisplayGeometry subConfig = null)
        {
            var width = subConfig?.Width ?? config?.Width ?? display?.Width ?? 0;
            var height = subConfig?.Height ?? config?.Height ?? display?.Height ?? 0;
            width += (display?.ImageGeometry?.Width ?? 0);
            height += (display?.ImageGeometry?.Height ?? 0);
            return new Size(width, height);
        }

    }
    #endregion Private helpers
}
