namespace MFDMF_Services.Configuration
{
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
        /// <param name="jsonContent"></param>
        /// <param name="displays"></param>
        /// <param name="category"></param>
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
        public IEnumerable<IModuleDefinition> LoadModulesConfigurationFile(string jsonContent, IEnumerable<IDisplayDefinition> displays, string category)
        {
            var modulesList = new List<IModuleDefinition>();
            try
            {
                if(jsonContent.Contains("\"modules\""))
                {
                    var moduleDefinitions = JsonConvert.DeserializeObject<ModuleDefinitions>(jsonContent);
                    var modules =  PreProcessModules(moduleDefinitions.Modules, displays, category);
                    modulesList.AddRange(modules);
                }
                else
                {
                    var moduleDefinitions = JsonConvert.DeserializeObject<List<ModuleDefinition>>(jsonContent);
                    var modules = PreProcessModules(moduleDefinitions, displays, category);
                    modulesList.AddRange(modules);
                }
                return modulesList;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to load {jsonContent} {ex}");
                throw;
            }
        }

		#endregion Public methods

		#region Private helpers

		/// <summary>
		/// Make sure the Hierarchy is setup
		/// </summary>
		/// <param name="modules"></param>
        /// <param name="displays"></param>
        /// <param name="category"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
		private IEnumerable<ModuleDefinition> PreProcessModules(List<ModuleDefinition> modules, IEnumerable<IDisplayDefinition> displays, string category)
        {
            foreach(var arg in modules) 
            {
                if (arg != null)
                {
                    arg.Enabled ??= true;
                    arg.FilePath ??= _settings.FilePath;

                    _logger.LogDebug($"Module: {arg.ModuleName} Display Name: {arg.DisplayName} processing...");
                    arg.Category = category;
                    arg?.Configurations?.ForEach(config =>
                    {
                        // Get the Display for the configuration and get the placement
                        var currentConfig = LoadBaseConfigurationDefinition(arg, config, displays, out var currentDisplay);   
                        
                        var placement = GetPlacementRect(currentConfig, currentDisplay);
                        currentConfig.Left = placement.X;
                        currentConfig.Top = placement.Y;
                        currentConfig.Width = placement.Width;
                        currentConfig.Height = placement.Height;
                                                
                        var croppingRect = GetCroppingRect(currentConfig, currentDisplay);
                        currentConfig.XOffsetStart ??= croppingRect.Left;
                        currentConfig.XOffsetFinish ??= croppingRect.Right;
                        currentConfig.YOffsetStart ??= croppingRect.Top;
                        currentConfig.YOffsetFinish ??= croppingRect.Bottom;

                        _logger.LogDebug($"Configuration {currentConfig.Name} using {currentDisplay?.Name ?? "Scratch"} Using file: {currentConfig?.FileName ?? "None"} Size: ({currentConfig.Width ?? 0},{currentConfig.Height ?? 0}) Location: ({currentConfig.Left ?? 0}, {currentConfig.Top ?? 0})");
                        _logger.LogDebug($"Cropping offsets ({currentConfig.XOffsetStart??0}, {currentConfig.YOffsetStart??0}) to ({currentConfig.XOffsetFinish??0}, {currentConfig.YOffsetFinish??0}) Opacity: {currentConfig.Opacity}");

                        ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(currentConfig, (subConfig) =>
                        {
                            var currentSubConfig = LoadBaseConfigurationDefinition(arg, subConfig, displays, out var currentSubDisplay, currentConfig);
                            currentSubConfig.Parent = config;

                            var placement = GetPlacementRect(currentConfig, currentSubDisplay, currentSubConfig);
                            currentSubConfig.Left = placement.X;
                            currentSubConfig.Top = placement.Y;
                            currentSubConfig.Width = placement.Width;
                            currentSubConfig.Height = placement.Height;

                            // The cropping offsets can originate from the sub configurations display offset geometry and from their parent configurations
                            var croppingRect = GetCroppingRect(currentConfig, currentDisplay, currentSubConfig);
                            currentSubConfig.XOffsetStart ??= croppingRect.Left;
                            currentSubConfig.XOffsetFinish ??= croppingRect.Right;
                            currentSubConfig.YOffsetStart ??= croppingRect.Top;
                            currentSubConfig.YOffsetFinish ??= croppingRect.Bottom;
                            currentSubConfig.Opacity ??= currentSubDisplay?.Opacity ?? currentConfig.Opacity;

                            _logger.LogDebug($"Configuration: {currentSubConfig.Name} using {currentSubDisplay?.Name ?? "Scratch"} Using file: {currentSubConfig?.FileName ?? "None"} Size: ({currentSubConfig.Width ?? 0},{currentSubConfig.Height ?? 0}) Location: ({currentSubConfig.Left ?? 0}, {currentSubConfig.Top ?? 0})");
                            _logger.LogDebug($"Cropping offsets: ({currentSubConfig.XOffsetStart ?? 0}, {currentSubConfig.YOffsetStart ?? 0}) to ({currentSubConfig.XOffsetFinish ?? 0}, {currentSubConfig.YOffsetFinish ?? 0}) Opacity: {currentSubConfig.Opacity}");
                            _logger.LogDebug($"Image properties: UseAsSwitch: {currentSubConfig.UseAsSwitch ?? false} MakeOpaque: {currentSubConfig.MakeOpaque ?? false} Enabled: {currentSubConfig.Enabled ?? false}");
                        });
                    });
                }
                yield return arg;
            };
        }

        /// <summary>
        /// Loads the base definition of the configuration using the configured displays 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="config"></param>
        /// <param name="displays"></param>
        /// <param name="mainDef"></param>
        /// <returns></returns>
        private IConfigurationDefinition LoadBaseConfigurationDefinition(IModuleDefinition arg,  IConfigurationDefinition config, IEnumerable<IDisplayDefinition> displays, out IDisplayDefinition mainDef,  IConfigurationDefinition parentDef = null)
        {
            IDisplayDefinition currentDisplay = null;
            IDisplayDefinition auxDisplay = null;

            // Get any of the displays where the configuration name contains their name
            var currentDisplays = displays?.Where(dd => dd.Name == config?.Name || (config?.Name?.Contains(dd.Name, StringComparison.CurrentCulture) ?? false));
            if (currentDisplays.Count() == 0)
            {
                currentDisplay = new DisplayDefinition(config);
            }
            else
            {
                // The primary display configuration MUST have it's name start with or equal the configuration name
                currentDisplay = currentDisplays.FirstOrDefault(cd => config.Name.StartsWith(cd.Name) || config.Name.Equals(cd.Name, StringComparison.CurrentCultureIgnoreCase));
                // The secondary display configuration MUST NOT have it's name start with the configuration name BUT must contain it
                auxDisplay = currentDisplays.FirstOrDefault(cd => !config.Name.StartsWith(cd.Name) && config.Name.Contains(cd.Name));
            }

            currentDisplay.XOffsetStart ??= (auxDisplay?.XOffsetStart ?? currentDisplay?.XOffsetStart);
            currentDisplay.XOffsetFinish ??= (auxDisplay?.XOffsetFinish ?? currentDisplay?.XOffsetFinish);
            currentDisplay.YOffsetStart ??= (auxDisplay?.YOffsetStart ?? currentDisplay?.YOffsetStart);
            currentDisplay.YOffsetFinish ??= (auxDisplay?.YOffsetFinish ?? currentDisplay?.YOffsetFinish);

            config.Logger = _logger;
            config.Enabled ??= arg.Enabled ?? config.Enabled ?? parentDef?.Enabled ?? false;
            config.UseAsSwitch ??= parentDef?.UseAsSwitch ?? false;
            config.ModuleName ??= arg?.ModuleName;
            config.FilePath ??= arg?.FilePath;
            config.FileName ??= arg?.FileName;
            config.MakeOpaque ??= false;
            config.Center ??= false;
            config.Opacity ??= 1.0F;
            config.Opacity ??= currentDisplay?.Opacity ?? 1.0F;
            config.RulerName = (_settings.ShowRulers ?? false) ? $"Ruler-{_settings.RulerSize ?? 0}" : null;
            var throttleType = (_settings?.UseCougar ?? false) ? "HC" : "WH";
            config.ThrottleType = throttleType;
            mainDef = currentDisplay;
            return config;
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
