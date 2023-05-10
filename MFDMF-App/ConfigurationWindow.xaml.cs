using MFDMF_Models;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
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

		private readonly ILogger? _logger;
		private readonly AppSettings _settings;
		private readonly IEnumerable<IDisplayDefinition> _displayDefinitions;
		private readonly IConfigurationProvider _configurationProvider;

		/// <summary>
		/// The configuration for this Window
		/// </summary>
		public IConfigurationDefinition Configuration { get; set; }
		/// <summary>
		/// Name of the selected subConfigurations
		/// </summary>
		public string? SubConfigurationNames { get; set; }
		/// <summary>
		/// Is the Window loaded?
		/// </summary>
		public bool IsWindowLoaded { get; protected set; }

		private List<string>? _selectedSubs => SubConfigurationNames?.Split('|')?.ToList();

		public IModuleDefinition ModuleDefinition { get; }
		
		public Dictionary<string, ImageDefinition> ImageDictionary { get; private set; }

		#region Folder definitions

		/// <summary>
		/// AppData 
		/// </summary>
		public static string? AppDataFolder => MainApp.AppDataFolder;
		
		/// <summary>
		/// Saved games
		/// </summary>
		public static string? SavedGamesFolder => MainApp.SavedGamesFolder;

		/// <summary>
		/// Cache folder for current Module
		/// </summary>
		public string CacheFolder => Path.Combine(AppDataFolder ?? "", Properties.Resources.BaseDataDirectory, "cache", Configuration.ModuleName);

		#endregion Folder definitions

		#endregion fields and properties

		#region Constructor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="module"></param>
		/// <param name="loggerFactory"><see cref="ILoggerFactory"/> used to create the <see cref="ILogger"/></param>
		/// <param name="settings">The <see cref="AppSettings"/> for the application</param>
		/// <param name="configurationProvider"></param>
		public ConfigurationWindow(IModuleDefinition module, IConfigurationDefinition configurationDefinition, ILoggerFactory loggerFactory, AppSettings settings, IConfigurationProvider configurationProvider)
		{
			ModuleDefinition = module;
			_settings = settings;
			_logger = loggerFactory?.CreateLogger(typeof(ConfigurationWindow));
			Configuration = configurationDefinition;
			_configurationProvider = configurationProvider;
			ImageDictionary = _configurationProvider.LoadConfigurationImages(module, configurationDefinition, Properties.Resources.THROTTLEKEY, Properties.Resources.HOTASKEY);
			_displayDefinitions = _configurationProvider.DisplayDefinitions;
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
			Width = Configuration?.Width ?? displayForConfig?.Width ?? 0;
			Height = Configuration?.Height ?? displayForConfig?.Height ?? 0;
			Left = Configuration?.Left ?? displayForConfig?.Left ?? 0;
			Top = Configuration?.Top ?? displayForConfig?.Top ?? 0;
			Opacity = Configuration?.Opacity ?? 1.0F;
			var status = $"Configuration: {Title} at ({Left},{Top}) for ({Width},{Height}) Created from: {displayForConfig?.ToReadableString() ?? "Scratch"}";
			_logger?.LogInformation(status);
			if (_settings.ShowTooltips ?? false)
			{
				ToolTip = status;
			}
			Topmost = displayForConfig?.AlwaysOnTop ?? false;
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
			while((controlGrid?.Children?.Count ?? 0) > 0)
			{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                controlGrid.Children.RemoveAt(0);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
			var imgMain = new System.Windows.Controls.Image()
			{
				Name = imageName
			};
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            controlGrid.Children.Add(imgMain);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return imgMain;
		}

		private void SaveOriginalImageIfRequired(Bitmap src)
		{
			if (_settings?.SaveCroppedImages ?? false)
			{
				using (var originalBitmap = new Bitmap(src.Width, src.Height))
				{
					using (var gg = Graphics.FromImage(originalBitmap))
					{
						gg.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
						gg.DrawImage(src, new Rectangle() { X = 0, Width = src.Width, Height = src.Height });
						CreateCroppingRectangle(Configuration, gg);
						_logger?.LogInformation($"Using appFolder: {AppDataFolder}");
						var cacheFile = Path.Combine(CacheFolder, $"{Configuration.Name}-Original.png");
						originalBitmap.Save(cacheFile);
					}
				}
			}
		}

		/// <summary>
		/// Checks to see if the current configuration is statically selected or selected via command line logic
		/// </summary>
		/// <param name="sc"></param>
		/// <returns></returns>
		private bool CheckForConfiguration(IConfigurationDefinition sc)
		{
			return (sc?.CheckForActiveSelectedSubConfiguration(_selectedSubs) ?? false) == true;
		}

		private void CreateCroppingRectangle(IConfigurationDefinition config, Graphics g)
		{
			g.DrawRectangle(Pens.Red, config.CroppingArea);
		}

		private void SaveFileAsKneeboardRefAsRequired(string path, IConfigurationDefinition config)
		{
			if ((_settings.CreateKneeboard ?? false) && !string.IsNullOrEmpty(ModuleDefinition.DCSName))
			{
				using var img = System.Drawing.Image.FromFile(path);
				var kneeBoardPath = Path.Combine(SavedGamesFolder ?? "", _settings.DcsSavedGamesPath, "Kneeboard", ModuleDefinition.DCSName);
				var kneeBoardFile = Path.Combine(kneeBoardPath, $"{ModuleDefinition.DCSName}-{config.Name}.png");
				if (!Directory.Exists(kneeBoardPath))
				{
					Directory.CreateDirectory(kneeBoardPath);
					_logger?.LogInformation($"Creating {kneeBoardPath}");
				}
				if (!File.Exists(kneeBoardFile) || (_settings.TurnOffCache ?? false))
				{
					using var kneeBoardBitmap = new Bitmap(768, 1024, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
					using (var gk = Graphics.FromImage(kneeBoardBitmap))
					{
						var cropRect = config.CroppingArea;
						gk.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
						gk.DrawImage(img, new Rectangle(0, 0, kneeBoardBitmap.Width, kneeBoardBitmap.Height), new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(img.Width, img.Height)), GraphicsUnit.Pixel);
					}
					kneeBoardBitmap.Save(kneeBoardFile);
				}
			}
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
		private void LoadImage(string cacheFile)
		{
			var imgMain = CreateNewImage("imgMain");
			imgMain.StretchDirection = StretchDirection.Both;
			imgMain.Stretch = Stretch.Fill;
			var imgSource = new Uri(cacheFile, UriKind.RelativeOrAbsolute);
			var src = new BitmapImage();
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
			var key = $"{Configuration.ModuleName}-{Configuration.Name}";
			if (InitializeWindow())
			{
				ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(Configuration, (config) =>
				{
					// See if the Configuration is selected
					if (CheckForConfiguration(config))
					{
						key = $"{Configuration.ModuleName}-{Configuration?.Name}-{config.Name}";
					}
				});
				var selectedImage = ImageDictionary[key];
				LoadImage(selectedImage.CacheFile);
				SaveFileAsKneeboardRefAsRequired(selectedImage.CacheFile, Configuration);
			}
			watch.Stop();
			_logger?.LogWarning($"Configuration {Configuration.ModuleName}-{Configuration.Name}-{SubConfigurationNames}: loaded in {watch.ElapsedMilliseconds} milliseconds");
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
			var rightButton = e.RightButton;
			var mousePos = System.Windows.Forms.Control.MousePosition;
			var currentRect = new Rectangle(mousePos.X, mousePos.Y, 1, 1);

			IDisplayDefinition? displayDef;
			var displayDefs = _displayDefinitions.Where(dd => currentRect.IntersectsWith(new Rectangle(dd?.Left ?? 0, dd?.Top ?? 0, dd?.Width ?? 0, dd?.Height ?? 0)));
			if (displayDefs.Count() > 1)
			{
				// Get the smallest config in the area
				displayDef = displayDefs.Aggregate((first, second) => ((first.Width ?? 0) * (first.Height ?? 0)) > ((second.Width ?? 0) * (second.Height ?? 0)) ? second : first);
			}
			else
			{
				displayDef = displayDefs.SingleOrDefault();
			}
			var relatedDisplays = _displayDefinitions.Where(dd => dd.Name.StartsWith(displayDef?.Name ?? ""));
			if (relatedDisplays.Count() > 1)
			{
				// Get the display definition with the longest name
				displayDef = relatedDisplays.Aggregate((first, second) => (first.Name.Length > second.Name.Length) ? first : second);
			}

			if (e.RightButton == MouseButtonState.Pressed)
			{
				switch (System.Windows.MessageBox.Show("Do you want to close this configuration window?", $"Close {Configuration?.DisplayName} in display {displayDef?.Name ?? "Not Specified"}", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
				{
					case MessageBoxResult.No:
					case MessageBoxResult.Cancel:
					case MessageBoxResult.None:
						break;
					case MessageBoxResult.OK:
					case MessageBoxResult.Yes:
						Close();
						break;
				}
			}
			else
			{
				var screen = GetScreen(this);
				var clientLeft = mousePos.X - (displayDef?.Left ?? 0);
				var clientTop = mousePos.Y - (displayDef?.Top ?? 0);
				System.Windows.MessageBox.Show($"({mousePos.X}, {mousePos.Y}) ({clientLeft}, {clientTop}) in {displayDef?.ToReadableString() ?? "None"} on Screen {screen?.DeviceName ?? "None"}", $"{Configuration.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
				base.OnMouseDown(e);
			}
		}

		#endregion Mouse events

		private static Screen GetScreen(Window window)
		{
			return Screen.FromHandle(new WindowInteropHelper(window).Handle);
		}

	}
}
