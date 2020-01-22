using MFDMF_Models.Models;
using MFDMFApp.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MFDMFApp
{
	/// <summary>
	/// Interaction logic for ConfigurationWindow.xaml
	/// </summary>
	public partial class ConfigurationWindow : Window
	{
		#region fields and properties

		private readonly ILogger _logger;
		private readonly AppSettings _settings;
		private readonly List<DisplayDefinition> _displayDefinitions;
		private DisplayDefinition _displayForConfig;

		/// <summary>
		/// The configuration for this Window
		/// </summary>
		public ConfigurationDefinition Configuration { get; set; }
		/// <summary>
		/// Path where the image file(s) for this Configuration may be found
		/// </summary>
		public object FilePath { get; set; }
		/// <summary>
		/// Name of the selected subConfiguration
		/// </summary>
		public string SubConfigurationName { get; set; }
		/// <summary>
		/// Is the Window loaded?
		/// </summary>
		public bool IsWindowLoaded { get; protected set; }

		#endregion fields and properties

		#region Constructor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="loggerFactory"><see cref="ILoggerFactory"/> used to create the <see cref="ILogger"/></param>
		/// <param name="displayDefinitions">Loaded display definitions <see cref="List{T}"/> of <see cref="DisplayDefinition"/></param>
		/// <param name="settings">The <see cref="AppSettings"/> for the application</param>
		public ConfigurationWindow(ILoggerFactory loggerFactory, List<DisplayDefinition> displayDefinitions, AppSettings settings)
		{
			_settings = settings;
			_logger = loggerFactory?.CreateLogger(typeof(ConfigurationWindow));
			_displayDefinitions = displayDefinitions;
			InitializeComponent();
		}

		#endregion Constructor

		#region Initialize method

		/// <summary>
		///  Uses the Configuration to set the properties for this MFD
		/// </summary>
		/// <returns></returns>
		private bool InitializeWindow()
		{
			// See if there is a Display that matches the name 
			_displayForConfig = _displayDefinitions?.FirstOrDefault(dd => dd.Name == Configuration?.Name || (Configuration?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
			Title = Configuration?.Name;
			ResizeMode = ResizeMode.NoResize;
			Width = Configuration?.Width ?? 0;
			Height = Configuration?.Height ?? 0;
			Opacity = Configuration?.Opacity ?? _displayForConfig?.Opacity ?? 1.0F;
			Configuration.XOffsetStart ??= _displayForConfig?.XOffsetStart;
			Configuration.XOffsetFinish ??= _displayForConfig?.XOffsetFinish;
			Configuration.YOffsetStart ??= _displayForConfig?.YOffsetStart;
			Configuration.YOffsetFinish ??= _displayForConfig?.YOffsetFinish;
			Left = (_displayForConfig?.Left ?? 0) + (Configuration?.Left ?? 0);
			Top = (_displayForConfig?.Top ?? 0) + (Configuration?.Top ?? 0);

			_logger.LogInformation($"Configuration: {Title} at ({Left},{Top}) for ({Width},{Height}) Created from: {_displayForConfig?.ToReadableString() ?? "Scratch"}");

			if (_settings.ShowTooltips ?? false)
			{
				ToolTip = new ToolTip() { Content = $"ToolTip - {Title} ({Left}, {Top}) for ({Width}, {Height}) from {_displayForConfig?.Name ?? "Scratch"} Opa: {Opacity}" };
			}
			return true;
		}

		#endregion Initialize method

		#region Image Loading

		/// <summary>
		/// Deletes all controls from the <see cref="Grid"/> and adds a new one
		/// </summary>
		/// <returns></returns>
		private System.Windows.Controls.Image CreateNewImage(string imageName)
		{
			var controlGrid = this.Content as Grid;
			while(controlGrid.Children.Count > 0)
			{
				controlGrid.Children.RemoveAt(0);
			}
			var imgMain = new System.Windows.Controls.Image()
			{
				Name = imageName
			};
			controlGrid.Children.Add(imgMain);
			return imgMain;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="config"></param>
		/// <returns></returns>
		private Bitmap Crop(Bitmap src, ConfigurationDefinition config)
		{
			var imageSize = new System.Drawing.Size((config.XOffsetFinish ?? 0) - (config.XOffsetStart ?? 0), (config.YOffsetFinish ?? 0) - (config.YOffsetStart ?? 0));
			var cropRect = new Rectangle(new System.Drawing.Point(config.XOffsetStart ?? 0, config.YOffsetStart ?? 0), imageSize);

			var newBitmap = new Bitmap(cropRect.Width, cropRect.Height);
			using (var g = Graphics.FromImage(newBitmap))
			{
				g.DrawImage(src, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), cropRect, GraphicsUnit.Pixel);
			}

			if (_settings.SaveCroppedImages ?? false)
			{
				var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Properties.Resources.BaseDataDirectory}cache\\{config.ModuleName}");
				var imagePrefix = $"{config.Name}-{config.XOffsetStart}-{config.XOffsetFinish}-{config.YOffsetStart}-{config.YOffsetFinish}";
				var cacheFile = Path.Combine(cacheFolder, $"{imagePrefix}.jpg");
				newBitmap.Save(cacheFile);
			}
			return newBitmap;
		}



		/// <summary>
		/// Superimposes one image on top of another 
		/// </summary>
		/// <param name="imageDictionary"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="Exception"></exception>
		private Bitmap Superimpose(Dictionary<string, Bitmap> imageDictionary)
		{
			// Based oon the current configuration we need to pull the main image
			Bitmap newBitmap;
			var key = $"{Configuration.ModuleName}-{Configuration.Name}";
			var mainImage = imageDictionary.FirstOrDefault(id => id.Key == key).Value;

			using (var cropped = Crop(mainImage, Configuration))
			{
				newBitmap = (Bitmap) SetOpacity(cropped, Configuration.Opacity);
			}

			_logger.LogInformation($"Configuration: {key} Image ({mainImage.Width}, {mainImage.Height}) Cropped to ({newBitmap.Width}, {newBitmap.Height})");

			if (newBitmap == null)
			{
				_logger.LogError($"Unable to find the image with a key of {key}");
				throw new ArgumentNullException(nameof(imageDictionary), $"Image with key: {key} was not found");
			}
			using(var g = Graphics.FromImage(newBitmap))
			{
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

				var currentConfig = Configuration;
				while ((currentConfig?.SubConfigurations?.Count ?? 0) > 0)
				{
					currentConfig?.SubConfigurations?.ForEach(subConfig =>
					{
						if ((subConfig.Enabled  ?? false && !(subConfig.UseAsSwitch ?? false)) || ((subConfig.UseAsSwitch ?? false) && (SubConfigurationName?.Equals(subConfig.Name, StringComparison.InvariantCultureIgnoreCase) ?? false)))
						{
							key = $"{subConfig.ModuleName}-{subConfig?.Parent?.Name}-{subConfig.Name}";
							var insetImage = imageDictionary.FirstOrDefault(id => id.Key == key).Value;
							using var croppedInset = Crop(insetImage, subConfig);
							if (croppedInset == null)
							{
								_logger.LogError($"Unable to find the image with a key of {key}");
								throw new ArgumentNullException(nameof(imageDictionary), $"Image with key: {key} was not found");
							}
							var insetBitmap = (Bitmap) SetOpacity(croppedInset, subConfig.Opacity);
							insetBitmap.MakeTransparent();
							int x = subConfig.Left;
							int y = subConfig.Top;
							g.DrawImage(insetBitmap, new Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(subConfig.Width, subConfig.Height)));
							var parentConfig = $"Module: {subConfig?.Parent?.ModuleName} Filename: {subConfig?.Parent?.FileName} Config: {subConfig?.Parent?.Name}";
							_logger.LogInformation($"Configuration: {key} Image ({insetImage.Width}, {insetImage.Height}) Cropped to ({insetBitmap.Width}, {insetBitmap.Height}) placed at ({x}, {y}) inside of {parentConfig} with an opacity: {subConfig.Opacity}");
						}
						currentConfig = subConfig;
					});
				}
			}
			return newBitmap;
		}

		/// <summary>
		/// Sets the Opacity of the Bitmap 
		/// </summary>
		/// <param name="src">Source <seealso cref="Bitmap"/></param>
		/// <param name="opacity">Opacity as a float</param>
		/// <returns></returns>
		private static System.Drawing.Image SetOpacity(System.Drawing.Image src, float? opacity)
		{
			var bitMap = new Bitmap(src.Width, src.Height);
			using (var gfx = Graphics.FromImage(bitMap))
			{
				var matrix = new ColorMatrix
				{
					Matrix33 = opacity ?? 1.0F
				};
				using var imageAttributes = new ImageAttributes();
				imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				gfx.DrawImage(src, new Rectangle(0, 0, bitMap.Width, bitMap.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, imageAttributes);
			}

			return bitMap;
		}

		/// <summary>
		/// Loads all of the files for a config and returns a dictionary of the results
		/// </summary>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="OutOfMemoryException"></exception>
		/// <exception cref="ArgumentException"></exception>
		private Dictionary<string, Bitmap> LoadBitmaps()
		{
			var imageDictionary = new Dictionary<string, Bitmap>();
			var fileSource = Path.Combine(Configuration.FilePath, Configuration.FileName);
			if(!File.Exists(fileSource))
			{
				throw new FileNotFoundException($"Unable to find the specified file at {Configuration.FilePath}", Configuration.FileName);
			}
			var bitMap = (Bitmap) System.Drawing.Image.FromFile(fileSource);
			imageDictionary.Add($"{Configuration.ModuleName}-{Configuration.Name}", bitMap);
			var currentConfig = Configuration;
			while((currentConfig?.SubConfigurations?.Count ?? 0) > 0)
			{
				currentConfig?.SubConfigurations?.ForEach(subConfig =>
				{
					fileSource = Path.Combine(subConfig.FilePath, subConfig.FileName);
					if (!File.Exists(fileSource))
					{
						throw new FileNotFoundException($"Unable to find the specified file at {subConfig.FilePath}", subConfig.FileName);
					}
					bitMap = (Bitmap) System.Drawing.Image.FromFile(fileSource);
					var key = $"{currentConfig.ModuleName}-{currentConfig.Name}-{subConfig.Name}";
					imageDictionary.Add(key, bitMap);
					currentConfig = subConfig;
				});
			}
			return imageDictionary;
		}


		/// <summary>
		/// Loads the configured image either from the test pattern, user's cache or from the original location
		/// </summary>
		private void LoadImage()
		{
			// Load the image dict
			var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Properties.Resources.BaseDataDirectory}cache\\{Configuration.ModuleName}");

			var imagePrefix = $"X-{Configuration.XOffsetStart}To{Configuration.XOffsetFinish}_Y-{Configuration.YOffsetStart}To{Configuration.YOffsetFinish}-{Configuration.Left}-{Configuration.Top}-{Configuration.Width}-{Configuration.Height}-{Configuration.Name}-{Configuration.Opacity ?? 1.0F}";
			if((SubConfigurationName?.Length ?? 0) > 0)
			{
				var subConfig = Configuration.SubConfigurations.FirstOrDefault(sub => sub.ModuleName == Configuration.ModuleName && sub.Name == SubConfigurationName);
				var subConfigOpacity = subConfig?.Opacity;
				imagePrefix += $"-{subConfig?.XOffsetStart ?? 0}To{subConfig?.XOffsetFinish ?? 0}_Y-{subConfig?.YOffsetStart ?? 0}To{subConfig?.YOffsetFinish ?? 0}-{subConfig?.Left ?? 0}-{subConfig?.Top ?? 0}-{subConfig?.Width ?? 0}-{subConfig?.Height ?? 0}-{SubConfigurationName}-{subConfigOpacity ?? 1.0F}";
			}
			var cacheFile = Path.Combine(cacheFolder, $"{imagePrefix}.jpg");

			if (!Directory.Exists(cacheFolder))
			{
				_logger.LogWarning($"Creating directory: {cacheFolder}");
				Directory.CreateDirectory(cacheFolder);
			}

			if (!File.Exists(cacheFile))
			{
				_logger.LogWarning($"Cache file NOT found: {cacheFile}");
				var imageDictionary = LoadBitmaps();
				_logger?.LogInformation($"Loaded {imageDictionary.Count} images for {Configuration.ModuleName}-{Configuration.Name}");
				var imageMain = Superimpose(imageDictionary);
				imageMain.Save(cacheFile);
				imageMain.Dispose();
				_logger.LogInformation($"Saved Cache file: {cacheFile}");
			}
			else
			{
				_logger?.LogInformation($"Found cache file: {cacheFile}");
			}

			var imgMain = CreateNewImage("imgMain");
			imgMain.StretchDirection = StretchDirection.Both;
			imgMain.Stretch = Stretch.Fill;
			var imgSource = new Uri(cacheFile, UriKind.RelativeOrAbsolute);
			BitmapImage src = new BitmapImage();
			src.BeginInit();
			src.UriSource = imgSource;
			src.CacheOption = BitmapCacheOption.OnLoad;
			src.EndInit();
			imgMain.Source = src;
			imgMain.Width = Width;
			imgMain.Height = Height;
			imgMain.Visibility = Visibility.Visible;
			IsWindowLoaded = true;
		}

		#endregion Image Loading

		#region Image saving

		/// <summary>
		/// Saves the Image in the specified format, default format is bmp
		/// </summary>
		/// <param name="retResult"><seealso cref="BitmapSource"/></param>
		/// <param name="cacheFolder"></param>
		/// <param name="cacheFile"></param>
		protected virtual void SaveImage(BitmapSource retResult, string cacheFolder, string cacheFile)
		{
			Directory.CreateDirectory(cacheFolder);
			retResult?.SaveToJpg(cacheFile);
			_logger.LogInformation($"Cropped image saved as {cacheFile}");
		}

		#endregion Image saving

		#region Window events

		/// <summary>
		/// This Window is loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			if (InitializeWindow())
			{
				LoadImage();
			}
			watch.Stop();
			_logger.LogInformation($"Configuration {Configuration.ModuleName}-{Configuration.Name}-{SubConfigurationName}: loaded in {watch.ElapsedMilliseconds} milliseconds");
		}

		/// <summary>
		/// This window is closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_logger?.LogDebug($"Configuration {Configuration?.Name} for Module {Configuration?.ModuleName} is closing");
		}

		/// <summary>
		/// This window is closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed(object sender, EventArgs e)
		{
			IsWindowLoaded = false;
			_logger?.LogInformation($"Configuration {Configuration?.Name} for Module {Configuration?.ModuleName} is unloaded");
		}

		#endregion Window events

	}
}
