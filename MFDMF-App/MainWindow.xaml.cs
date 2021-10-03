using MFDMF_Models.Comparer;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MFDMFApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string UnableToLoad = "Unable to load the configuration!";
		#region IoC Injected fields
		private readonly AppSettings _settings;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<MainWindow> _logger;
		private readonly StartOptions _startOptions;
		private readonly IConfigurationProvider _configurationProvider;
		#endregion IoC Injected fields

		#region Private fields
		/// <summary>
		/// List of all created <see cref="ConfigurationWindow"/> Windows
		/// </summary>
		private SortedList<string, ConfigurationWindow> _windowList;
		/// <summary>
		/// Currently selected Module
		/// </summary>
		private IModuleDefinition _selectedModule;
		/// <summary>
		/// The name of the module that was passed in
		/// </summary>
		private string _module;
		/// <summary>
		/// The name of the sub-module that was passed
		/// </summary>
		private string _subModule;
		/// <summary>
		/// Is the current module valid?
		/// </summary>
		public bool IsValid { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		public static string ThrottleKey => Properties.Resources.THROTTLEKEY;
		/// <summary>
		/// 
		/// </summary>
		public static string HotasKey => Properties.Resources.HOTASKEY;
		#endregion Private fields

		#region Constructor
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"><see cref="AppSettings"/> loaded via Dependency Injection</param>
		/// <param name="loggerFactory"><see cref="ILoggerFactory"/> logging factory loaded via Dependency Injection</param>
		/// <param name="configurationProvider"></param>
		public MainWindow(IOptions<AppSettings> settings, ILoggerFactory loggerFactory, IConfigurationProvider configurationProvider)
		{
			InitializeComponent();
			_settings = settings?.Value;
			_loggerFactory = loggerFactory;
			_configurationProvider = configurationProvider;
			_logger = _loggerFactory.CreateLogger<MainWindow>();
			_windowList = new SortedList<string, ConfigurationWindow>();
			_startOptions = (StartOptions) ((MainApp)Application.Current).Host.Services.GetService(typeof(StartOptions));
		}
		#endregion Constructor
		
		#region Modules and SubModule processing

		/// <summary>
		/// Creates all the windows for the <see cref="ConfigurationDefinition"/> definitions
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void CreateWindows()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			_logger?.LogWarning($"Creating configuration {_selectedModule?.DisplayName}");
			_selectedModule?.Configurations?.ForEach(config =>
			{
				if (config?.Enabled ?? false)
				{
					var configWindow = new ConfigurationWindow(_selectedModule, config, _loggerFactory, _settings, _configurationProvider)
					{
						SubConfigurationNames = _subModule
					};
					configWindow.Show();
					if (configWindow.IsWindowLoaded)
					{
						_windowList.Add(config.Name, configWindow);
						configWindow.Visibility = Visibility.Visible;
					}
					else
					{
						configWindow?.Close();
					}
				}
				else
				{
					_logger?.LogWarning($"Configuration: {config.ToReadableString()} Disabled");
				}
			});
			watch.Stop();
			_logger?.LogWarning($"Module {_selectedModule?.DisplayName ?? "None"}: SubModule(s): {_subModule} loaded in {watch.ElapsedMilliseconds} milliseconds");

		}

		/// <summary>
		/// Destroys all active <see cref="ConfigurationWindow"/>
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void DestroyWindows()
		{
			_windowList.ToList().ForEach(mfd =>
			{
				if (mfd.Value.IsLoaded)
				{
					mfd.Value.Hide();
					mfd.Value.Close();
				}
			});
			_windowList.Clear();
			_logger?.LogInformation(Properties.Resources.WindowListCleared);
		}

		/// <summary>
		/// Sets up the main window
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void SetupWindow()
		{
			var moduleList = _settings.ModuleItems.ToList();
			moduleList?.Sort(new ModuleDefinitionComparer());
			cbModules.ItemsSource = moduleList;
			cbModules.DisplayMemberPath = "DisplayName";
			cbModules.SelectedValuePath = "ModuleName";
		}

	
		/// <summary>
		/// Used to change the selected module 
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="forceReload">If true then the module selection is forced</param>
		public void ChangeSelectedModule(string moduleName, bool forceReload)
		{
			if(forceReload)
			{
				cbModules.SelectedIndex = -1;
			}
			if (moduleName != (string)cbModules?.SelectedValue)
			{
				cbModules.SelectedValue = moduleName;
			}

			if(cbModules.SelectedIndex == -1)
			{
				IsValid = false;
				_logger?.LogError($"Unable to find a configuration named {moduleName}");
				throw new ArgumentOutOfRangeException(nameof(moduleName));
			}
			else
			{
				IsValid = true;
			}

			UpdateMenu();
		}

		/// <summary>
		/// Event for the module selection change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				DestroyWindows();
				if (e.AddedItems.Count == 0)
				{
					_selectedModule = null;
				}
				else
				{
					_selectedModule = (ModuleDefinition)e.AddedItems[0];
					CreateWindows();
				}
				_logger?.LogInformation($"Module selected {_selectedModule?.ToReadableString()}");
				UpdateMenu();
			}
			catch (IndexOutOfRangeException ioorx)
			{
				_logger?.LogError($"Not able to determine selected module {ioorx}");
			}
		}

		#endregion Modules and SubModule processing

		#region Window events

		/// <summary>
		/// Window is closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed(object sender, EventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.MainWindowClosed);
		}

		/// <summary>
		/// Window is closing so clean up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.MainWindowClosing);
			DestroyWindows();
		}

		/// <summary>
		/// Window is loaded and ready to be setup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();
				ReloadConfiguration();
				watch.Stop();
				_logger?.LogInformation($"Main window loaded in: {watch.ElapsedMilliseconds} milliseconds");
			}
			catch(Exception ex)
			{
				string errorMessage = $"{UnableToLoad}, Specified module may not be loaded. FileSpec: {_settings.FileSpec} FilePath: {_settings.FilePath} Request Module: {_module}";
				_logger?.LogError(ex,errorMessage);
				MessageBox.Show(errorMessage, "Error Loading!", MessageBoxButton.OK, MessageBoxImage.Hand);
				Close();
			}
		}

		#endregion Window events

		#region Menu Item processing

		/// <summary>
		/// User requested to close
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FileMenuItem_Click(object sender, RoutedEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.UserRequestedAppClose);
			Close();
		}

		/// <summary>
		/// Clears the cache of all PNG files
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="PlatformNotSupportedException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		private void ClearCache_Click(object sender, RoutedEventArgs e)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			_logger?.LogInformation(Properties.Resources.UserRequestedCacheClear);
			DestroyWindows();
			var appDataFolder = MainApp.AppDataFolder;
			_logger.LogInformation($"Using appFolder: {appDataFolder}");
			var cacheFolder = Path.Combine(appDataFolder, Properties.Resources.BaseDataDirectory, "cache");
			if (Directory.Exists(cacheFolder))
			{
				var cacheParent = new DirectoryInfo(cacheFolder);
				var dirs = cacheParent.EnumerateDirectories().ToList();
				dirs.ForEach(dir =>
				{
					try
					{
						dir.Delete(true);
						_logger?.LogInformation($"Deleted cache at {dir.FullName}");
					}
					catch (Exception ex)
					{
						_logger?.LogError($"Unable to delete cache file: {dir.FullName}, Exception: {ex}");
					}
				});
			}
			watch.Stop();
			_logger?.LogInformation($"Clear cache took: {watch.ElapsedMilliseconds} milliseconds");
			ReloadConfiguration(true);
		}
		private void EditConfiguration_Click(object sender, RoutedEventArgs e)
		{
			var newEditConfig = new Configuration(_settings);
			newEditConfig.ShowDialog();
		}

		private void Modules_Click(object sender, RoutedEventArgs e)
		{
			var modulesList = new Modules(_settings);
			modulesList.ShowDialog();
		}

		private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(MainApp.VersionString);
		}

		/// <summary>
		/// Menu item to relaod the current configuration
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReloadConfiguration_Click(object sender, RoutedEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.UserRequestedReload);
			ReloadConfiguration(true);
		}

		/// <summary>
		/// Used to always load the configuration
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void ReloadConfiguration(bool forceReload = false)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var mainApp = Application.Current as MainApp;
			mainApp.ReloadConfiguration();
			_module = _startOptions?.ModuleName;
			_subModule = _startOptions?.SubModuleName;
			var module = (string)cbModules?.SelectedValue;
			DestroyWindows();
			SetupWindow();
			var selectedModule = module ??= _module ??= _settings.DefaultConfiguration;
			if (!string.IsNullOrEmpty(selectedModule))
			{
				_logger?.LogInformation($"Loading module {selectedModule}");
				ChangeSelectedModule(selectedModule, forceReload);
			}
			watch.Stop();
			_logger?.LogInformation($"Reloaded configuration in: {watch.ElapsedMilliseconds} milliseconds");
		}

		private void UpdateMenu()
		{
			mnuEditConfiguration.IsEnabled = false;
			mnuShowModuleList.IsEnabled = false;
			mnuUnloadModule.IsEnabled = _selectedModule != null;
			mnuReloadAllCache.IsEnabled = _selectedModule == null;
		}

		private void UnLoadModule_Click(object sender, RoutedEventArgs e)
		{
			cbModules.SelectedIndex = -1;
		}

		private void ReloadAllCache_Click(object sender, RoutedEventArgs e)
		{
			var bitmapDictionary = _configurationProvider.ReloadCacheForAllModulesAsync(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), ThrottleKey, HotasKey, true).Result;
			var loadedKeys = string.Join(',', bitmapDictionary.Keys.Select(key => key));
			MessageBox.Show(this, loadedKeys, "Bitmaps loaded");
		}

		#endregion Menu Item processing
	}
}
