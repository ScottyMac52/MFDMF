using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
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
		private readonly List<IDisplayDefinition> _displayDefinitions;

		/// <summary>
		/// The configuration for this Window
		/// </summary>
		public IConfigurationDefinition Configuration { get; set; }
		/// <summary>
		/// Name of the selected subConfigurations
		/// </summary>
		public string SubConfigurationNames { get; set; }
		/// <summary>
		/// Is the Window loaded?
		/// </summary>
		public bool IsWindowLoaded { get; protected set; }

		private List<string> _selectedSubs => SubConfigurationNames?.Split('|')?.ToList();


		#endregion fields and properties

		#region Constructor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="loggerFactory"><see cref="ILoggerFactory"/> used to create the <see cref="ILogger"/></param>
		/// <param name="displayDefinitions">Loaded display definitions <see cref="List{T}"/> of <see cref="DisplayDefinition"/></param>
		/// <param name="settings">The <see cref="AppSettings"/> for the application</param>
		public ConfigurationWindow(ILoggerFactory loggerFactory, List<IDisplayDefinition> displayDefinitions, AppSettings settings)
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
			var displayForConfig = _displayDefinitions?.FirstOrDefault(dd => dd.Name == Configuration?.Name || (Configuration?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
			Title = Configuration?.Name;
			ResizeMode = ResizeMode.NoResize;
			Width = Configuration?.Width ?? 0;
			Height = Configuration?.Height ?? 0;
			Left = Configuration?.Left ?? 0;
			Top = Configuration?.Top ?? 0;
			Opacity = Configuration?.Opacity ?? 1.0F;
			var status = $"Configuration: {Title} at ({Left},{Top}) for ({Width},{Height}) Created from: {displayForConfig?.ToReadableString() ?? "Scratch"}";
			_logger?.LogInformation(status);
			if (_settings.ShowTooltips ?? false)
			{
				ToolTip = status;
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
			var controlGrid = Content as Grid;
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
		/// Crops the image to the specified dimensions 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="config"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="Exception"></exception>
		private Bitmap Crop(Bitmap src, IConfigurationDefinition config)
		{
			var error = false;
			var imageSize = new System.Drawing.Size((config.XOffsetFinish ?? 0) - (config.XOffsetStart ?? 0), (config.YOffsetFinish ?? 0) - (config.YOffsetStart ?? 0));
			var cropRect = new Rectangle(new System.Drawing.Point(config.XOffsetStart ?? 0, config.YOffsetStart ?? 0), imageSize);

			if((config?.Width ??  0) < 0)
			{
				_logger?.LogError($"Width cannot be less than zero for config {config?.ToReadableString()}");
				error = true;
			}

			if ((config?.Height ?? 0) < 0)
			{
				_logger?.LogError($"Height cannot be less than zero for config {config?.ToReadableString()}");
				error = true;
			}

			if (!error)
			{

				var newBitmap = new Bitmap(config?.Width ?? 0, config?.Height ?? 0);
				using (var g = Graphics.FromImage(newBitmap))
				{
					g.DrawImage(src, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), cropRect, GraphicsUnit.Pixel);
				}
				return newBitmap;
			}
			else
			{
				throw new ArgumentException($"Unable to Crop bitmap for {config.ToReadableString()}. Pleasse check the log.");
			}
		}
			   
		/// <summary>
		/// Superimposes one image on top of another 
		/// </summary>
		/// <param name="imageDictionary"></param>
		/// <returns></returns>
		/// <exception cref="System.Runtime.InteropServices.ExternalException"></exception>
		/// <exception cref="PlatformNotSupportedException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="Exception"></exception>
		private Bitmap Superimpose(Dictionary<string, Bitmap> imageDictionary)
		{
			// Based oon the current configuration we need to pull the main image
			var currentConfig = Configuration;
			var key = $"{currentConfig.ModuleName}-{currentConfig.Name}";
			var mainImage = imageDictionary.FirstOrDefault(id => id.Key == key).Value;
			var newBitmap = ProcessBitmap(currentConfig, key, mainImage);
			if (newBitmap == null)
			{
				_logger?.LogError($"Unable to find the image with a key of {key}");
				throw new ArgumentNullException(nameof(imageDictionary), $"Image with key: {key} was not found");
			}
			using (var g = Graphics.FromImage(newBitmap))
			{
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

				ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(currentConfig, (config) =>
				{
					// See if the Coonfiguration is selected
					if (CheckForConfiguration(config, _selectedSubs))
					{
						key = $"{currentConfig.ModuleName}-{currentConfig?.Name}-{config.Name}";
						var insetImage = imageDictionary.FirstOrDefault(id => id.Key == key).Value;
						var croppedInsetBitmap = ProcessBitmap(config, key, insetImage);
						if (croppedInsetBitmap == null)
						{
							_logger?.LogError($"Unable to find the image with a key of {key}");
							throw new ArgumentNullException(nameof(imageDictionary), $"Image with key: {key} was not found");
						}
						var origin = new System.Drawing.Point(config?.Left ?? 0, config?.Top ?? 0);
						g.DrawImage(croppedInsetBitmap, new Rectangle(origin, new System.Drawing.Size(config?.Width ?? 0, config?.Height ?? 0)));
						var parentConfig = $"Module: {config?.Parent?.ModuleName} Filename: {config?.Parent?.FileName} Config: {config?.Parent?.Name}";
						_logger?.LogInformation($"Configuration: {key} Image ({insetImage.Width}, {insetImage.Height}) Cropped to ({croppedInsetBitmap.Width}, {croppedInsetBitmap.Height}) placed at ({origin.X}, {origin.Y}) inside of {parentConfig} with an opacity: {config?.Opacity ?? 1.0F}");
					}
				});

				if (_settings.ShowRulers ?? false)
				{
					_logger?.LogInformation($"Drawing rulers on {currentConfig.ToReadableString()} at {_settings?.RulerSize ?? 0} pixels");
					CreateRulers(currentConfig, g);
				}
			}
			return newBitmap;
		}

		/// <summary>
		/// Checks to see if the current configuration is statically selected or selected via command line logic
		/// </summary>
		/// <param name="sc"></param>
		/// <param name="specifiedSubConfigs"></param>
		/// <returns></returns>
		private static bool CheckForConfiguration(IConfigurationDefinition sc, List<string> specifiedSubConfigs)
		{
			return (sc?.CheckForActiveSelectedSubConfiguration(specifiedSubConfigs) ?? false) == true;
		}


		private void CreateRulers(IConfigurationDefinition config, Graphics g)
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
					var textPoint = new PointF(x - 10, (float) yCenter + 10);
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

				if(y % 100 == 0)
				{
					var textPoint = new PointF(xCenter + 10, y-5);
					g.DrawString($"{y}", System.Drawing.SystemFonts.DefaultFont, System.Drawing.Brushes.Red, textPoint);
				}
			}
		}

		/// <summary>
		/// Process the Bitmap
		/// </summary>
		/// <param name="config"></param>
		/// <param name="key"></param>
		/// <param name="mainImage"></param>
		/// <returns></returns>
		private Bitmap ProcessBitmap(IConfigurationDefinition config, string key, Bitmap mainImage)
		{
			Bitmap newBitmap;
			using (var cropped = Crop(mainImage, config))
			{
				newBitmap = (Bitmap)SetOpacity(cropped, config.Opacity ?? 1.0F);
				if (!(config?.MakeOpaque ?? false))
				{
					newBitmap.MakeTransparent();
				}
			}

			if ((_settings.SaveCroppedImages ?? false) && !(_settings.TurnOffCache ?? false))
			{
				var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Properties.Resources.BaseDataDirectory}cache\\{config.ModuleName}");
				var imagePrefix = config.ToReadableString();
				var cacheFile = Path.Combine(cacheFolder, $"{imagePrefix}-Crop.jpg");
				newBitmap.Save(cacheFile);
			}
			_logger?.LogInformation($"Configuration: {key} Image ({mainImage.Width}, {mainImage.Height}) Cropped to ({newBitmap.Width}, {newBitmap.Height})");
			return newBitmap;
		}


		/// <summary>
		/// Sets the Opacity of the Bitmap 
		/// </summary>
		/// <param name="src">Source <seealso cref="Bitmap"/></param>
		/// <param name="opacity">Opacity as a float</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="Exception"></exception>
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
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		private Dictionary<string, Bitmap> LoadBitmaps()
		{
			var imageDictionary = new Dictionary<string, Bitmap>();
			var replacementValue = (_settings?.UseCougar ?? false) ? "HC" : "WH";
			var filePath = Configuration.FilePath.Contains("%", StringComparison.InvariantCultureIgnoreCase) ? Environment.ExpandEnvironmentVariables(Configuration.FilePath) : Configuration.FilePath;
			var fileSource = Path.Combine(filePath, Configuration.FileName.Replace(Properties.Resources.THROTTLEKEY, replacementValue, StringComparison.InvariantCulture));
			if(!File.Exists(fileSource))
			{
				if ((_settings?.UseCougar ?? false) == true)
				{
					fileSource = Path.Combine(filePath, Configuration.FileName.Replace(Properties.Resources.THROTTLEKEY, Properties.Resources.HOTASKEY, StringComparison.InvariantCulture));
				}
				if (!File.Exists(fileSource))
				{
					throw new FileNotFoundException($"Unable to find the specified file at {fileSource}");
				}
			}
			_logger?.LogInformation($"Loading file: {fileSource} for Configuration {Configuration.ModuleName}-{Configuration.Name}");
			var bitMap = (Bitmap) System.Drawing.Image.FromFile(fileSource);
			imageDictionary.Add($"{Configuration.ModuleName}-{Configuration.Name}", bitMap);
			var currentConfig = Configuration;

			ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(currentConfig, (subConfig) =>
			{
				filePath = subConfig.FilePath.Contains("%", StringComparison.InvariantCultureIgnoreCase) ? Environment.ExpandEnvironmentVariables(subConfig.FilePath) : subConfig.FilePath;
				fileSource = Path.Combine(filePath, subConfig.FileName);
				if (!File.Exists(fileSource))
				{
					throw new FileNotFoundException($"Unable to find the specified file at {filePath}", subConfig.FileName);
				}
				_logger?.LogInformation($"Loading file: {fileSource} for {subConfig.ToReadableString()}");
				bitMap = (Bitmap)System.Drawing.Image.FromFile(fileSource);
				var key = $"{currentConfig.ModuleName}-{currentConfig.Name}-{subConfig.Name}";
				imageDictionary.Add(key, bitMap);
			});

			return imageDictionary;
		}


		/// <summary>
		/// Loads the configured image either from the test pattern, user's cache or from the original location
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.ExternalException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="OutOfMemoryException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		private void LoadImage()
		{
			// Load the image dict
			var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Properties.Resources.BaseDataDirectory}cache\\{Configuration.ModuleName}");
			var cacheFile = Path.Combine(cacheFolder, $"{Configuration.GetImagePrefix(_selectedSubs)}-{Left}-{Top}-{Width}-{Height}.jpg");

			if (!Directory.Exists(cacheFolder))
			{
				_logger?.LogWarning($"Creating directory: {cacheFolder}");
				Directory.CreateDirectory(cacheFolder);
			}

			if (!File.Exists(cacheFile) || (_settings.TurnOffCache ?? false))
			{
				_logger?.LogWarning($"Cache file NOT found: {cacheFile}");
				var imageDictionary = LoadBitmaps();
				_logger?.LogInformation($"Loaded {imageDictionary.Count} images for {Configuration.ModuleName}-{Configuration.Name}");

				using(var imageMain = Superimpose(imageDictionary))
				{
					imageMain.Save(cacheFile);
				}
				_logger?.LogInformation($"Saved Cache file: {cacheFile}");
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

		#region Window events

		/// <summary>
		/// This Window is loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="System.Runtime.InteropServices.ExternalException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="OutOfMemoryException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			if (InitializeWindow())
			{
				LoadImage();
			}
			watch.Stop();
			_logger?.LogInformation($"Configuration {Configuration.ModuleName}-{Configuration.Name}-{SubConfigurationNames}: loaded in {watch.ElapsedMilliseconds} milliseconds");
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

		#region Mouse events

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			var mousePos = System.Windows.Forms.Control.MousePosition;
			var currentRect = new Rectangle(mousePos.X, mousePos.Y, 1, 1);
			var screen = GetScreen(this);
			var currentDisplay = _displayDefinitions?.FirstOrDefault(dd => currentRect.IntersectsWith(new Rectangle(dd?.Left ?? 0, dd?.Top ?? 0, dd?.Width ?? 0, dd?.Height ?? 0)));
			var clientLeft = mousePos.X - (currentDisplay?.Left ?? 0);
			var clientTop = mousePos.Y - (currentDisplay?.Top ?? 0);
			System.Windows.MessageBox.Show($"({mousePos.X}, {mousePos.Y}) ({clientLeft}, {clientTop}) in {currentDisplay?.ToReadableString() ?? "None"} on Screen {screen?.DeviceName ?? "None"}", $"{Configuration.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
			base.OnMouseDown(e);
		}

		#endregion Mouse events

		private static Screen GetScreen(Window window)
		{
			return Screen.FromHandle(new WindowInteropHelper(window).Handle);
		}

	}
}
