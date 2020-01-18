using MFDMF_Models.Models;
using MFDMFApp.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
		private byte[] _imageBytes;
		private bool _isTestPattern;

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
		/// <param name="imageBytes">Image bytes to use for the image for load</param>
		public ConfigurationWindow(ILoggerFactory loggerFactory, List<DisplayDefinition> displayDefinitions, AppSettings settings, byte[] imageBytes = null)
		{
			_settings = settings;
			_logger = loggerFactory?.CreateLogger(typeof(ConfigurationWindow));
			_displayDefinitions = displayDefinitions;
			InitializeComponent();
			_isTestPattern = (imageBytes?.Length ?? 0) > 0;
			_imageBytes = imageBytes;
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
			Opacity = Configuration.Opacity ??= _displayForConfig.Opacity ??= 1.0F;
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
		private Image CreateNewImage()
		{
			var controlGrid = this.Content as Grid;
			while(controlGrid.Children.Count > 0)
			{
				controlGrid.Children.RemoveAt(0);
			}
			var imgMain = new Image()
			{
				Name = "imgMain",
				StretchDirection = StretchDirection.Both,
				Stretch = Stretch.Fill
			};
			controlGrid.Children.Add(imgMain);
			return imgMain;
		}

		/// <summary>
		/// Loads the configured image either from the test pattern, user's cache or from the original location
		/// </summary>
		private void LoadImage()
		{
			var imgMain = CreateNewImage();

			if (_isTestPattern)
			{
				IsWindowLoaded = false;
				var image = new BitmapImage();
				using (var mem = new MemoryStream(_imageBytes))
				{
					mem.Position = 0;
					image.BeginInit();
					image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.UriSource = null;
					image.StreamSource = mem;
					image.EndInit();
				}
				image.Freeze();
				imgMain.Source = image;
				imgMain.Width = Width;
				imgMain.Height = Height;
				imgMain.Visibility = Visibility.Visible;
				IsWindowLoaded = true;
				return;
			}

			try
			{
				if (Configuration?.Enabled ?? false)
				{
					IsWindowLoaded = false;
					var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Vyper Industries\\MFDMF\\cache\\{Configuration.ModuleName}");
					var imagePrefix = $"X-{Configuration.XOffsetStart}To{Configuration.XOffsetFinish}_Y-{Configuration.YOffsetStart}To{Configuration.YOffsetFinish}";
					var cacheFile = Path.Combine(cacheFolder, $"{imagePrefix}_{Configuration.Name}.jpg");
					if (File.Exists(cacheFile))
					{
						BitmapImage bitmap = new BitmapImage();
						bitmap.BeginInit();
						bitmap.CacheOption = BitmapCacheOption.OnLoad;
						bitmap.UriSource = new Uri(cacheFile);
						bitmap.EndInit();
						imgMain.Source = bitmap;
						imgMain.Width = Width;
						imgMain.Height = Height;
						imgMain.Visibility = Visibility.Visible;
					}
					else
					{
						imgMain.Source = GetBitMapSource(Configuration, cacheFolder, cacheFile);
						imgMain.Width = Width;
						imgMain.Height = Height;
						imgMain.Visibility = Visibility.Visible;
					}
					IsWindowLoaded = true;
				}
				else
				{
					_logger.LogWarning($"Configuration {Configuration.Name} for Module {Configuration.ModuleName} is disabled");
				}
			}
			catch (Exception ex)
			{
				_logger?.LogError($"Unable to load {Configuration.ToReadableString()}", ex);
				throw;
			}
			finally
			{
				if (imgMain != null)
				{
					_logger?.LogInformation($"Configuration {Configuration.ToReadableString()} is loaded");
				}
				else
				{
					_logger?.LogWarning($"Configuration was not loaded -> {Configuration.ToReadableString()}");
					Close();
				}
			}
		}

		/// <summary>
		/// Loads the image from the cache or crops it from the source image
		/// </summary>
		/// <typeparam name="T">Must be derived from <seealso cref="ConfigurationBaseDefinition"/></typeparam>
		/// <param name="configSource">The configuration source, derived from <seealso cref="ConfigurationBaseDefinition"/> </param>
		/// <param name="cacheFolder">Location of the cache of images</param>
		/// <param name="cacheFile">Full path to the requested file in the cache</param>
		/// <returns><seealso cref="BitmapSource"/></returns>
		private BitmapSource GetBitMapSource<T>(T configSource, string cacheFolder, string cacheFile)
			where T : ConfigurationDefinition
		{
			BitmapSource bitmapSource = null;

			if (File.Exists(cacheFile))
			{
				_logger.LogInformation($"Cache file found: {cacheFile}");
				Stream imageStreamSource = new FileStream(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read);
				PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				bitmapSource = decoder.Frames[0];
			}
			else
			{
				if (!Directory.Exists(cacheFolder))
				{
					Directory.CreateDirectory(cacheFolder);
				}
				var filePath = Path.Combine(configSource.FilePath, configSource.FileName);

				var imgSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
				var width = (configSource.XOffsetFinish ?? 0) - (configSource.XOffsetStart ?? 0);
				var height = (configSource.YOffsetFinish ?? 0) - (configSource.YOffsetStart ?? 0);
				Int32Rect offSet = new Int32Rect(configSource.XOffsetStart ?? 0, configSource.YOffsetStart ?? 0, width, height);
				BitmapImage src = new BitmapImage();
				src.BeginInit();
				src.UriSource = imgSource;
				src.CacheOption = BitmapCacheOption.OnLoad;
				src.EndInit();
				var croppedBitmap = new CroppedBitmap(src, offSet);
				var noAlphaSource = new FormatConvertedBitmap();
				noAlphaSource.BeginInit();
				noAlphaSource.Source = croppedBitmap;
				noAlphaSource.DestinationFormat = PixelFormats.Bgr24;
				//noAlphaSource.AlphaThreshold = 0;
				noAlphaSource.EndInit();
				SaveImage(noAlphaSource, cacheFolder, cacheFile);
				bitmapSource = noAlphaSource;
				return noAlphaSource;
			}

			return bitmapSource;
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
			if (InitializeWindow())
			{
				LoadImage();
				_logger?.LogDebug($"Loading the configuration for {Configuration.Name} from Module {Configuration.ModuleName} as {Title} ({Left}, {Top}) for ({Width}, {Height})");
			}
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
