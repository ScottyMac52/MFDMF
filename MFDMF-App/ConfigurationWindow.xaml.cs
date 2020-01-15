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

		public bool IsTestPattern { get; set; }

		#endregion fields and properties

		#region Constructor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="loggerFactory"></param>
		/// <param name="displayDefinitions"></param>
		/// <param name="settings"></param>
		public ConfigurationWindow(ILoggerFactory loggerFactory, List<DisplayDefinition> displayDefinitions, AppSettings settings, bool useTestPattern = false)
		{
			_settings = settings;
			_logger = loggerFactory?.CreateLogger(typeof(ConfigurationWindow));
			_displayDefinitions = displayDefinitions;
			InitializeComponent();
			IsTestPattern = useTestPattern;
		}

		#endregion Constructor

		#region Protected methods

		/// <summary>
		///  Uses the Configuration to set the properties for this MFD
		/// </summary>
		/// <returns></returns>
		protected bool InitializeWindow()
		{
			// See if there is a Display that matches the name 
			_displayForConfig = _displayDefinitions?.FirstOrDefault(dd => dd.Name == Configuration?.Name || (Configuration?.Name?.StartsWith(dd.Name, StringComparison.CurrentCulture) ?? false));
			Title = Configuration?.Name;
			ResizeMode = ResizeMode.NoResize;
			Width = Configuration?.Width ?? 0;
			Height = Configuration?.Height ?? 0;
			Opacity = Configuration.Opacity ??= _displayForConfig.Opacity ??= 1.0F;
			Configuration.XOffsetStart ??= _displayForConfig.XOffsetStart;
			Configuration.XOffsetFinish ??= _displayForConfig.XOffsetFinish;
			Configuration.YOffsetStart ??= _displayForConfig.YOffsetStart;
			Configuration.YOffsetFinish ??= _displayForConfig.YOffsetFinish;
			Left = (_displayForConfig?.Left ?? 0) + (Configuration?.Left ?? 0);
			Top = (_displayForConfig?.Top ?? 0) + (Configuration?.Top ?? 0);

			_logger.LogInformation($"Configuration: {Title} at ({Left},{Top}) for ({Width},{Height}) Created from: {_displayForConfig?.ToReadableString() ?? "Scratch"}");

			if (_settings.ShowTooltips ?? false)
			{
				ToolTip = new ToolTip() { Content = $"ToolTip - {Title} ({Left}, {Top}) for ({Width}, {Height}) from {_displayForConfig?.Name ?? "Scratch"} Opa: {Opacity}" };
			}
			return true;
		}

		#endregion Protected methods

		#region Image Loading

		/// <summary>
		/// Loads an image from a byte[]
		/// </summary>
		/// <param name="imageData"></param>
		/// <returns></returns>
		private BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0) return null;
			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData))
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
			return image;
		}

		/// <summary>
		/// Loads the configured image either from the user's cache or from the original location
		/// </summary>
		private void LoadImage()
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
		}

		public void LoadImageFromBytes(byte[] imageBytes)
		{
			LoadImage(imageBytes);
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
			where T : ConfigurationBaseDefinition
		{
			BitmapSource bitmapSource = null;
			string filePath = string.Empty;

			try
			{
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
					filePath = Path.Combine(configSource.FilePath, configSource.FileName);

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
			}
			catch (Exception ex)
			{
				_logger?.LogError($"Unable to load {configSource.ToReadableString()}", ex);
				throw;
			}
			finally
			{
				if (bitmapSource != null)
				{
					_logger?.LogInformation($"Configuration {configSource.ToReadableString()} is loaded");
				}
				else
				{
					_logger?.LogWarning($"Configuration was not loaded -> {configSource.ToReadableString()}");
					Close();
				}
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
				if (Configuration?.Enabled ?? false)
				{
					IsWindowLoaded = true;
					if (!IsTestPattern)
					{
						LoadImage();
						_logger?.LogDebug($"Loading the configuration for {Configuration.Name} from Module {Configuration.ModuleName} as {Title} ({Left}, {Top}) for ({Width}, {Height})");
					}
				}
				else
				{
					_logger?.LogInformation($"Configuration for {Configuration.Name} for Module {Configuration.ModuleName} is currently disabled in configuration");
				}
			}
		}

		/// <summary>
		/// This window is closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			IsWindowLoaded = false;
			imgMain = null;
			imgInsert = null;
			_logger?.LogDebug($"Configuration {Configuration?.Name} for Module {Configuration?.ModuleName} is closing");
		}

		/// <summary>
		/// This window is closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed(object sender, EventArgs e)
		{
			_logger?.LogInformation($"Configuration {Configuration?.Name} for Module {Configuration?.ModuleName} is unloaded");
		}

		#endregion Window events

	}
}
