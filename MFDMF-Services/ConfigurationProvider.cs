using MFDMF_Models;

namespace MFDMF_Services
{
    using MFDMF_Models.Interfaces;
    using MFDMF_Models.Models;
    using MFDMF_Services.Configuration;
    using MFDMF_Services.Displays;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfigurationLoadingService _configurationLoadingService;
        private readonly IDisplayConfigurationService _displayConfigurationService;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly IEnumerable<IDisplayDefinition> _displayDefinitions;

        /// <summary>
        /// Gets the Users Saved games folder
        /// </summary>
        private static string SavedGamesFolder => GetSpecialFolder("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}", Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "SavedGames"));

        public IEnumerable<IDisplayDefinition> DisplayDefinitions => _displayDefinitions;

        private string _cacheFolder;

        /// <summary>
        /// Ctor injects <see cref="IConfigurationLoadingService"/> and <see cref="IDisplayConfigurationService"/> Services
        /// </summary>
        /// <param name="configurationLoadingService"></param>
        /// <param name="displayConfigurationService"></param>
        public ConfigurationProvider(IConfigurationLoadingService configurationLoadingService, IDisplayConfigurationService displayConfigurationService, ILogger<ConfigurationProvider> logger, IOptions<AppSettings> settings)
        {
            _configurationLoadingService = configurationLoadingService;
            _displayConfigurationService = displayConfigurationService;
            _logger = logger;
            _settings = settings.Value;
            _displayDefinitions = _displayConfigurationService.LoadDisplaysAsync().Result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IModuleDefinition>> GetModulesAsync(string path, string fileSpec)
        {
            var modules = await LoadModulesAsync(path, fileSpec).ConfigureAwait(false);
            return modules;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, ImageDefinition>> ReloadCacheForAllModulesAsync(string path, string throttleKey, string hotasKey, bool loadKneeboards = false)
        {
            var bitmapDictionary = new Dictionary<string, ImageDefinition>();
            var modulesToProceeResponse = await LoadModulesAsync(path, _settings.FileSpec ?? "*.json").ConfigureAwait(false);
            var modulesToProcess = modulesToProceeResponse?.ToList();
            modulesToProcess.ForEach(module =>
            {
                var moduleDictionary = LoadModuleImages(module, throttleKey, hotasKey);
                moduleDictionary.Keys.ToList().ForEach(key =>
                {
                    bitmapDictionary.Add(key, moduleDictionary[key]);
                });
            });
            return bitmapDictionary;
        }


        /// <inheritdoc/>
        public Dictionary<string, ImageDefinition> LoadModuleImages(IModuleDefinition module, string throttleKey, string hotasKey, bool forceReload = false)
        {
            var bitmapDictionary = new Dictionary<string, ImageDefinition>();

            _logger.LogWarning($"Processing {module.DisplayName}...");
            CheckCacheFolder(module);
            var configs = string.Join(',', module.Configurations.Select(config => $"{config.ModuleName}-{config.Name}"));
            _logger.LogInformation($"Processing Configurations: {configs}");
            module.Configurations?.ForEach(config =>
            {
                var configurationBitmaps = LoadConfigurationImages(module, config, throttleKey, hotasKey, forceReload);
                configurationBitmaps?.Keys?.ToList().ForEach(key =>
                {
                    bitmapDictionary.Add(key, configurationBitmaps[key]);
                });
            });

            return bitmapDictionary;
        }

        public Dictionary<string, ImageDefinition> LoadConfigurationImages(IModuleDefinition module, IConfigurationDefinition config, string throttleKey, string hotasKey, bool forceReload = false)
        {
            var bitmapDictionary = new Dictionary<string, ImageDefinition>();
            CheckCacheFolder(module);

            // Load all source bitmaps (base + one-per-subconfig at any depth)
            var imageDictionary = LoadBitmaps(config, throttleKey, hotasKey);

            // Base image (no sub mods)
            var mainKey = $"{module.ModuleName}-{config.Name}";
            if (!imageDictionary.TryGetValue(mainKey, out var mainImage))
            {
                _logger?.LogError($"Missing base image for {mainKey}");
                return bitmapDictionary;
            }

            var baseTemplate = Path.Combine(_cacheFolder, $"{config.GetImagePrefix(new List<string>())}");
            var baseCacheFile = $"{baseTemplate}.png";
            var baseExists = File.Exists(baseCacheFile);
            try
            {
#pragma warning disable CA1416 // Validate platform compatibility
                if (!baseExists || forceReload)
                {
                    using var g = Graphics.FromImage(mainImage);
                    CreateRulersAsRequired(config, g);
                    mainImage.Save(baseCacheFile);
                }
#pragma warning restore CA1416 // Validate platform compatibility
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, $"Error creating base cache file {baseCacheFile} for {mainKey}");
            }
            bitmapDictionary[mainKey] = new ImageDefinition { Bitmap = mainImage, CacheFile = baseCacheFile, Key = mainKey };

            // For each top-level subConfig, compose: base + sub + all descendants of that sub
            foreach (var topSub in config.SubConfigurations ?? new List<ConfigurationDefinition>())
            {
                // Build name list for file prefix: sub + all of its descendants
                var names = new List<string> { topSub.Name };
                names.AddRange(GetDescendantNames(topSub));

                // Compute cache path for this composite
                var fileTemplate = Path.Combine(_cacheFolder, $"{config.GetImagePrefix(names)}");
                var subCacheFile = $"{fileTemplate}.png";
                var subExists = File.Exists(subCacheFile);

                Bitmap composite;
                if (subExists && !forceReload)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    composite = (Bitmap)Image.FromFile(subCacheFile);
#pragma warning restore CA1416 // Validate platform compatibility
                }
                else
                {
                    composite = ComposeTopLevelSelectionBitmap(mainImage, config, topSub, imageDictionary);
                    try
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        composite.Save(subCacheFile);
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"Error saving composite to {subCacheFile} for {module.ModuleName}-{config.Name}-{topSub.Name}");
                    }
                }

                // Register under the top-level sub key
                var topKey = $"{module.ModuleName}-{config.Name}-{topSub.Name}";
                var imgDef = new ImageDefinition { Bitmap = composite, CacheFile = subCacheFile, Key = topKey };
                bitmapDictionary[topKey] = imgDef;

                // Also register under each descendant key so lookups like ...-BIT_Selected resolve to the same composite
                foreach (var dn in names.Skip(1)) // skip the topSub itself
                {
                    var childKey = $"{module.ModuleName}-{config.Name}-{dn}";
                    if (!bitmapDictionary.ContainsKey(childKey))
                        bitmapDictionary[childKey] = imgDef;
                }
            }

            return bitmapDictionary;
        }

        // Returns all descendant names of the given top-level sub (excludes the top itself)
        private static IEnumerable<string> GetDescendantNames(IConfigurationDefinition top)
        {
            var names = new List<string>();
            ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(top, node =>
            {
                if (!ReferenceEquals(node, top))
                    names.Add(node.Name);
            });
            return names;
        }

        // Composes base + top-level sub + all descendants of that sub
        private Bitmap ComposeTopLevelSelectionBitmap(Bitmap mainImage, IConfigurationDefinition rootConfig, IConfigurationDefinition topSub, Dictionary<string, Bitmap> imageDictionary)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var unit = GraphicsUnit.Pixel;
            var result = (Bitmap)mainImage.Clone(mainImage.GetBounds(ref unit), PixelFormat.Format24bppRgb);

            // Draw the top-level sub
            var topKey = $"{rootConfig.ModuleName}-{rootConfig.Name}-{topSub.Name}";
            if (imageDictionary.TryGetValue(topKey, out var topBitmap))
            {
                using var g = Graphics.FromImage(result);
                g.CompositingMode = CompositingMode.SourceOver;
                var loc = (topSub.Center ?? false)
                    ? topSub.GetCenterTo(rootConfig)
                    : new Point(topSub.Left ?? rootConfig.Left ?? 0, topSub.Top ?? rootConfig.Top ?? 0);
                var rect = new Rectangle(loc, new Size(topSub.Width ?? 0, topSub.Height ?? 0));
                g.DrawImage(topBitmap, rect);

                // Draw all descendants of the top-level sub
                ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(topSub, sub =>
                {
                    if (ReferenceEquals(sub, topSub)) return;
                    var subKey = $"{rootConfig.ModuleName}-{rootConfig.Name}-{sub.Name}";
                    if (!imageDictionary.TryGetValue(subKey, out var subBitmap)) return;

                    using var gs = Graphics.FromImage(result);
                    gs.CompositingMode = CompositingMode.SourceOver;
                    var subLoc = (sub.Center ?? false)
                        ? sub.GetCenterTo(topSub)
                        : new Point(sub.Left ?? topSub.Left ?? rootConfig.Left ?? 0, sub.Top ?? topSub.Top ?? rootConfig.Top ?? 0);
                    var subRect = new Rectangle(subLoc, new Size(sub.Width ?? 0, sub.Height ?? 0));
                    gs.DrawImage(subBitmap, subRect);
                });

                CreateRulersAsRequired(rootConfig, g);
            }
#pragma warning restore CA1416 // Validate platform compatibility

            return result;
        }

        private void CreateRulersAsRequired(IConfigurationDefinition config, Graphics g)
        {
            if (_settings.ShowRulers ?? false)
            {
                var xCenter = (config.Width ?? 0) / 2;
                var yCenter = (config.Height ?? 0) / 2;

#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                g.DrawLine(Pens.Red, new System.Drawing.Point(0, yCenter), new System.Drawing.Point(config.Width ?? 0, yCenter));
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility

                for (int x = 0; x < (config.Width ?? 0); x++)
                {
                    if (x % (_settings.RulerSize ?? 0) == 0)
                    {
                        var startPoint = new System.Drawing.Point(x, yCenter - 10);
                        var endPoint = new System.Drawing.Point(x, yCenter + 10);
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                        g.DrawLine(Pens.OrangeRed, startPoint, endPoint);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
                    }

                    if (x % 100 == 0)
                    {
                        var textPoint = new PointF(x - 10, (float)yCenter + 10);
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                        g.DrawString($"{x}", System.Drawing.SystemFonts.DefaultFont, System.Drawing.Brushes.Red, textPoint);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                }

#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                g.DrawLine(Pens.Red, new System.Drawing.Point(xCenter, 0), new System.Drawing.Point(xCenter, config.Height ?? 0));
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility

                for (int y = 0; y < (config.Height ?? 0); y++)
                {
                    if (y % (_settings.RulerSize ?? 0) == 0)
                    {
                        var startPoint = new System.Drawing.Point(xCenter - 10, y);
                        var endPoint = new System.Drawing.Point(xCenter + 10, y);
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                        g.DrawLine(Pens.OrangeRed, startPoint, endPoint);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
                    }

                    if (y % 100 == 0)
                    {
                        var textPoint = new PointF(xCenter + 10, y - 5);
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                        g.DrawString($"{y}", System.Drawing.SystemFonts.DefaultFont, System.Drawing.Brushes.Red, textPoint);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                }
            }
        }

        private void CheckCacheFolder(IModuleName moduleName)
        {
            _cacheFolder = Path.Combine(SavedGamesFolder, "Vyper Industries", "MFDMF", "cache", moduleName.ModuleName);
            if (!Directory.Exists(_cacheFolder))
            {
                _logger?.LogWarning($"Creating directory: {_cacheFolder}");
                Directory.CreateDirectory(_cacheFolder);
            }
        }


        /// <summary>
        /// Crops the src <see cref="Bitmap"/> using the configuration <see cref="IConfigurationDefinition"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private Bitmap Crop(Bitmap src, IDisplayGeometry displayGeometry, IOffsetGeometry cropping)
        {
            var cropRect = new Rectangle(cropping.CroppingStart, new Size(cropping.CroppedWidth, cropping.CroppedHeight));
            _logger.LogDebug($"Creating bitmap {displayGeometry?.Width ?? 0} * {displayGeometry?.Height ?? 0}");
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            var newBitmap = new Bitmap(displayGeometry?.Width ?? cropping.CroppedWidth, displayGeometry?.Height ?? cropping.CroppedHeight, PixelFormat.Format24bppRgb);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            using (var g = Graphics.FromImage(newBitmap))
            {
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                var matrix = new ColorMatrix
                {
                    Matrix33 = cropping.Opacity ?? 1.0F
                };
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                using var imageAttributes = new ImageAttributes();
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platformCompatibility
#pragma warning disable CA1416 // Validate platform compatibility
                g.DrawImage(src, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), cropRect.Left, cropRect.Top, cropRect.Width, cropRect.Height, GraphicsUnit.Pixel, imageAttributes);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
            }
#pragma warning restore CA1416 // Validate platform compatibility
            return newBitmap;
        }

        private static string GetSpecialFolder(string folderName, string defaultValue = null)
        {
            var regKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
            var regKeyValue = folderName;
#pragma warning disable CA1416 // Validate platform compatibility
            return (string)Registry.GetValue(regKey, regKeyValue, defaultValue);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private async Task<IEnumerable<IModuleDefinition>> LoadModulesAsync(string path, string fileSpec)
        {
            var items = new List<IModuleDefinition>();
            var dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.GetFiles(fileSpec))
            {
                var jsonContent = File.ReadAllText(file.FullName);
                var moduleDefintions = _configurationLoadingService.LoadModulesConfigurationFile(jsonContent, _displayDefinitions, dirInfo.Name);
                items.AddRange(moduleDefintions);
            }

            foreach (var directory in dirInfo.GetDirectories())
            {
                items.AddRange(await LoadModulesAsync(directory.FullName, fileSpec).ConfigureAwait(false));
            }
            return items;
        }

        /// <summary>
        /// Loads all of the files for a config and returns a dictionary of the results
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private Dictionary<string, Bitmap> LoadBitmaps(IConfigurationDefinition configurationDefinition, string throttleKey, string hotasKey)
        {
            string filePath, fileSource;
#pragma warning disable CA1416 // Validate platform compatibility
            var imageDictionary = new Dictionary<string, Bitmap>();
            (filePath, fileSource) = GetImageFilename(configurationDefinition, throttleKey, hotasKey);
            _logger?.LogInformation($"Loading file: {fileSource} for Configuration {configurationDefinition.ModuleName}-{configurationDefinition.Name} Throttle:{throttleKey} HOTAS:{hotasKey}");
            var bitMap = (Bitmap)Image.FromFile(fileSource, true);
            var key = $"{configurationDefinition.ModuleName}-{configurationDefinition.Name}";
            using var croppedBitmap = Crop(bitMap, configurationDefinition, configurationDefinition);
            var pageUnit = GraphicsUnit.Pixel;
            imageDictionary.Add(key, (Bitmap)croppedBitmap.Clone(croppedBitmap.GetBounds(ref pageUnit), PixelFormat.Format24bppRgb));
#pragma warning restore CA1416 // Validate platform compatibility
            var currentConfig = configurationDefinition;

            ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(currentConfig, (subConfig) =>
            {
                filePath = subConfig.FilePath.Contains("%", StringComparison.InvariantCultureIgnoreCase) ? Environment.ExpandEnvironmentVariables(subConfig.FilePath) : subConfig.FilePath;
                fileSource = Path.Combine(filePath, subConfig.FileName);
                if (!File.Exists(fileSource))
                {
                    throw new FileNotFoundException($"Unable to find the specified file at {filePath}", subConfig.FileName);
                }
                _logger?.LogInformation($"Loading file: {fileSource} for {subConfig}");
#pragma warning disable CA1416 // Validate platform compatibility
                bitMap = (Bitmap)Image.FromFile(fileSource);
                var key = $"{currentConfig.ModuleName}-{currentConfig.Name}-{subConfig.Name}";
                using var croppedBitmap = Crop(bitMap, subConfig, subConfig);
                imageDictionary.Add(key, (Bitmap)croppedBitmap.Clone());
#pragma warning restore CA1416 // Validate platform compatibility
            });

            return imageDictionary;
        }

        private (string, string) GetImageFilename(IConfigurationDefinition configurationDefinition, string throttleKey, string hotasKey)
        {
            var replacementValue = (_settings?.UseCougar ?? false) ? "HC" : "WH";
            var filePath = configurationDefinition.FilePath.Contains("%", StringComparison.InvariantCultureIgnoreCase) ? Environment.ExpandEnvironmentVariables(configurationDefinition.FilePath) : configurationDefinition.FilePath;
            var fileSource = Path.Combine(filePath, configurationDefinition.FileName.Replace(throttleKey, replacementValue, StringComparison.InvariantCulture));
            if (!File.Exists(fileSource))
            {
                if ((_settings?.UseCougar ?? false) == true)
                {
                    fileSource = Path.Combine(filePath, configurationDefinition.FileName.Replace(throttleKey, hotasKey, StringComparison.InvariantCulture));
                }
                if (!File.Exists(fileSource))
                {
                    _logger.LogCritical($"Unable to find file: {fileSource}");
                    throw new FileNotFoundException($"Unable to find the specified file at {fileSource}");
                }
            }
            return (filePath, fileSource);
        }

        private static List<List<ConfigurationDefinition>> GetAllSubConfigCombinations(List<ConfigurationDefinition> subConfigs)
        {
            var result = new List<List<ConfigurationDefinition>>();
            int count = subConfigs.Count;
            int max = 1 << count; // 2^count

            for (int i = 0; i < max; i++)
            {
                var combo = new List<ConfigurationDefinition>();
                for (int j = 0; j < count; j++)
                {
                    if ((i & (1 << j)) != 0)
                        combo.Add(subConfigs[j]);
                }
                result.Add(combo);
            }
            return result;
        }

        private Bitmap GenerateCompositeImage(IConfigurationDefinition config, List<ConfigurationDefinition> subConfigs, Dictionary<string, Bitmap> imageDictionary)
        {
            var graphicsUnit = GraphicsUnit.Pixel;
            var baseKey = $"{config.ModuleName}-{config.Name}";
            var baseImage = imageDictionary[baseKey];
            var resultBitmap = (Bitmap)baseImage.Clone(baseImage.GetBounds(ref graphicsUnit), PixelFormat.Format24bppRgb);

            using (var g = Graphics.FromImage(resultBitmap))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                foreach (var subConfig in subConfigs)
                {
                    var subKey = $"{config.ModuleName}-{config.Name}-{subConfig.Name}";
                    if (imageDictionary.TryGetValue(subKey, out var subImage))
                    {
                        var newLocation = (subConfig.Center ?? false) ? subConfig.GetCenterTo(config) : new Point(subConfig.Left ?? config.Left ?? 0, subConfig.Top ?? config.Top ?? 0);
                        var croppingArea = new Rectangle(newLocation, new Size(subConfig.Width ?? 0, subConfig.Height ?? 0));
                        g.DrawImage(subImage, croppingArea);
                    }
                }
                CreateRulersAsRequired(config, g);
            }
            return resultBitmap;
        }
    }
}
