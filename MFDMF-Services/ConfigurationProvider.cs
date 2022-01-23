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
			var imageDictionary = LoadBitmaps(config, throttleKey, hotasKey);
			_logger.LogInformation($"Configuration: {config.Name} has {imageDictionary.Count} images.");
			var fileTemplate = Path.Combine(_cacheFolder, $"{config.GetImagePrefix(new List<string>() { })}");
			var cacheFile = $"{fileTemplate}.png";
			var cacheFileExists = File.Exists(cacheFile);
			var key = $"{module.ModuleName}-{config.Name}";
			imageDictionary.TryGetValue(key, out var mainImage);
			_logger.LogDebug($"Processing image {key}");

			config?.SubConfigurations?.ForEach(subConfig =>
			{
				Bitmap currentConfig;
				var key = $"{module.ModuleName}-{config.Name}-{subConfig.Name}";
				var fileTemplate = Path.Combine(_cacheFolder, $"{config.GetImagePrefix(new List<string>() { subConfig.Name })}");
				var cacheFile = $"{fileTemplate}.png";
				var fileExists = File.Exists(cacheFile);
				if (fileExists)
				{
					currentConfig = (Bitmap)Image.FromFile(cacheFile);
				}
				else
				{
					currentConfig = (Bitmap)mainImage.Clone();
					if (imageDictionary.TryGetValue(key, out var sourceImage))
					{
						using (var g = Graphics.FromImage(currentConfig))
						{
							g.CompositingMode = CompositingMode.SourceOver;
							var newLocation = (subConfig.Center ?? false) ? subConfig.GetCenterTo(config) : new Point(subConfig.Left ?? config.Left ?? 0, subConfig.Top ?? config.Top ?? 0);
							var croppingArea = new Rectangle(newLocation, new Size(subConfig.Width ?? 0, subConfig.Height ?? 0));
							_logger.LogDebug($"Using image: {key} to paint configuration {subConfig.Name} for {subConfig}");
							g.DrawImage(sourceImage, croppingArea);

							ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(subConfig, (subsubConfig) =>
							{
								var key = $"{module.ModuleName}-{config.Name}-{subsubConfig.Name}";
								if (imageDictionary.TryGetValue(key, out var sourceImage))
								{
									using var graphicsSuperimposed = Graphics.FromImage(currentConfig);
									graphicsSuperimposed.CompositingMode = CompositingMode.SourceOver;
									var newLocation = (subsubConfig.Center ?? false) ? subsubConfig.GetCenterTo(subConfig) : new Point(subsubConfig.Left ?? subConfig.Left ?? config.Left ?? 0, subsubConfig.Top ?? subConfig.Top ?? config.Top ?? 0);
									var croppingArea = new Rectangle(newLocation, new Size(subsubConfig.Width ?? 0, subsubConfig.Height ?? 0));
									_logger.LogDebug($"Using image: {key} to paint configuration {subsubConfig.Name} for {subsubConfig}");
									graphicsSuperimposed.DrawImage(sourceImage, croppingArea);
								}
							});
						}
						using var graphics = Graphics.FromImage(currentConfig);
						CreateRulersAsRequired(config, graphics);
						currentConfig.Save(cacheFile);
					}
				}
				var bitMapCache = new ImageDefinition() { Bitmap = currentConfig, CacheFile = cacheFile, Key = key };
				bitmapDictionary.Add(key, bitMapCache);
			});

			if (!cacheFileExists || forceReload)
			{
				using var graphics = Graphics.FromImage(mainImage);
				CreateRulersAsRequired(config, graphics);
				_logger.LogDebug($"Creating cache file {cacheFile}");
				mainImage.Save($"{fileTemplate}.png");
			}
			var bitMapCache = new ImageDefinition() { Bitmap = mainImage, CacheFile = cacheFile, Key = key };
			bitmapDictionary.Add(key, bitMapCache);
			var mainKey = $"{module.ModuleName}-{config.Name}";
			return bitmapDictionary;
		}

		private void CreateRulersAsRequired(IConfigurationDefinition config, Graphics g)
		{
			if (_settings.ShowRulers ?? false)
			{
				var xCenter = (config.Width ?? 0) / 2;
				var yCenter = (config.Height ?? 0) / 2;

				g.DrawLine(Pens.Red, new System.Drawing.Point(0, yCenter), new System.Drawing.Point(config.Width ?? 0, yCenter));

				for (int x = 0; x < (config.Width ?? 0); x++)
				{
					if (x % (_settings.RulerSize ?? 0) == 0)
					{
						var startPoint = new System.Drawing.Point(x, yCenter - 10);
						var endPoint = new System.Drawing.Point(x, yCenter + 10);
						g.DrawLine(Pens.OrangeRed, startPoint, endPoint);
					}

					if (x % 100 == 0)
					{
						var textPoint = new PointF(x - 10, (float)yCenter + 10);
						g.DrawString($"{x}", System.Drawing.SystemFonts.DefaultFont, System.Drawing.Brushes.Red, textPoint);
					}
				}

				g.DrawLine(Pens.Red, new System.Drawing.Point(xCenter, 0), new System.Drawing.Point(xCenter, config.Height ?? 0));

				for (int y = 0; y < (config.Height ?? 0); y++)
				{
					if (y % (_settings.RulerSize ?? 0) == 0)
					{
						var startPoint = new System.Drawing.Point(xCenter - 10, y);
						var endPoint = new System.Drawing.Point(xCenter + 10, y);
						g.DrawLine(Pens.OrangeRed, startPoint, endPoint);
					}

					if (y % 100 == 0)
					{
						var textPoint = new PointF(xCenter + 10, y - 5);
						g.DrawString($"{y}", System.Drawing.SystemFonts.DefaultFont, System.Drawing.Brushes.Red, textPoint);
					}
				}
			}
		}


		// public Dictionary<string, ImageDefinition> 

		private void CheckCacheFolder(IModuleName moduleName)
		{
			_cacheFolder = Path.Combine(GetSpecialFolder("AppData", null), "Vyper Industries", "MFDMF", "cache", moduleName.ModuleName);
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
			var newBitmap = new Bitmap(displayGeometry?.Width ?? 0, displayGeometry?.Height ?? 0, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(newBitmap))
			{
				var matrix = new ColorMatrix
				{
					Matrix33 = cropping.Opacity ?? 1.0F
				};
				using var imageAttributes = new ImageAttributes();
				imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				g.DrawImage(src, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), cropRect.Left, cropRect.Top, cropRect.Width, cropRect.Height, GraphicsUnit.Pixel, imageAttributes);
			}

			if (!(cropping.MakeOpaque ?? false))
			{
				newBitmap.MakeTransparent(Color.White);
			}

			return newBitmap;
		}

		private string GetSpecialFolder(string folderName, string defaultValue = null)
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
			var imageDictionary = new Dictionary<string, Bitmap>();
			(filePath, fileSource) = GetImageFilename(configurationDefinition, throttleKey, hotasKey);
			_logger?.LogInformation($"Loading file: {fileSource} for Configuration {configurationDefinition.ModuleName}-{configurationDefinition.Name}");
			var bitMap = (Bitmap)Image.FromFile(fileSource);
			var key = $"{configurationDefinition.ModuleName}-{configurationDefinition.Name}";
			using var croppedBitmap = Crop(bitMap, configurationDefinition, configurationDefinition);
			imageDictionary.Add(key, (Bitmap)croppedBitmap.Clone());
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
				bitMap = (Bitmap)Image.FromFile(fileSource);
				var key = $"{currentConfig.ModuleName}-{currentConfig.Name}-{subConfig.Name}";
				using var croppedBitmap = Crop(bitMap, subConfig, subConfig);
				imageDictionary.Add(key, (Bitmap)croppedBitmap.Clone());
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
					throw new FileNotFoundException($"Unable to find the specified file at {fileSource}");
				}
			}
			return (filePath, fileSource);
		}

	}
}
