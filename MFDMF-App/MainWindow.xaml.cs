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
		private readonly ILogger<MainWindow> _logger;
		private readonly StartOptions? _startOptions;
		private readonly IConfigurationProvider _configurationProvider;
		#endregion IoC Injected fields

		#region Private fields
		/// <summary>
		/// List of all created <see cref="ConfigurationWindow"/> Windows
		/// </summary>
		private readonly SortedList<string, ConfigurationWindow>? _windowList;
		/// <summary>
		/// Currently selected Module
		/// </summary>
		private IModuleDefinition? _selectedModule;
		/// <summary>
		/// The name of the module that was passed in
		/// </summary>
		private string? _module;
		/// <summary>
		/// The name of the sub-module that was passed
		/// </summary>
		private string? _subModule;
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
		public MainWindow(IOptions<AppSettings> settings, ILogger<MainWindow> logger, IConfigurationProvider configurationProvider)
		{
			InitializeComponent();
			_settings = settings.Value;
			_logger = logger;
			_configurationProvider = configurationProvider;
			_windowList = new SortedList<string, ConfigurationWindow>();
			_startOptions = (StartOptions?) ((MainApp)Application.Current).Host?.Services.GetService(typeof(StartOptions));

			this.LocationChanged += MainWindow_LocationChanged;
		}

		private void MainWindow_LocationChanged(object? sender, EventArgs e)
		{
			SaveWindowPosition();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveWindowPosition();
			_logger?.LogInformation(Properties.Resources.MainWindowClosing);
			DestroyWindows();
		}

		private void SaveWindowPosition()
		{
			var appDataPath = MainApp.AppDataFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var settingsFile = Path.Combine(appDataPath, "windowposition.json");
			var positionSettings = new WindowPositionSettings
			{
				MainWindowLeft = this.Left,
				MainWindowTop = this.Top
			};
			var json = System.Text.Json.JsonSerializer.Serialize(positionSettings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(settingsFile, json);
		}

		private void RestoreWindowPosition()
		{
			var appDataPath = MainApp.AppDataFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var settingsFile = Path.Combine(appDataPath, "windowposition.json");
			if (File.Exists(settingsFile))
			{
				var json = File.ReadAllText(settingsFile);
				var positionSettings = System.Text.Json.JsonSerializer.Deserialize<WindowPositionSettings>(json);
				if (positionSettings?.MainWindowLeft != null && positionSettings?.MainWindowTop != null)
				{
					this.Left = positionSettings.MainWindowLeft.Value;
					this.Top = positionSettings.MainWindowTop.Value;
				}
			}
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

			// select the enabled configurations for the module
			var eneabledConfigurations = _selectedModule?.Configurations?.Where(config => config.Enabled == true)?.ToList();

            eneabledConfigurations?.ForEach(config =>
			{
#pragma warning disable CS8604 // Possible null reference argument.
                var configWindow = new ConfigurationWindow(_selectedModule, config, _settings, _configurationProvider)
				{
					SubConfigurationNames = _subModule
				};
#pragma warning restore CS8604 // Possible null reference argument.
                configWindow.Show();
				if (configWindow.IsWindowLoaded && configWindow.IsEnabled)
				{
					_windowList?.Add(config.Name, configWindow);
					configWindow.Visibility = Visibility.Visible;
				}
				else
				{
                    _logger?.LogWarning($"Configuration: {config} Disabled or there was a problem loading the window, see the log for more information.");
                    configWindow?.Close();
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
			_windowList?.ToList().ForEach(mfd =>
			{
				if (mfd.Value.IsLoaded)
				{
					var controlGrid = mfd.Value.Content as Grid;
					controlGrid?.Children?.Clear();
					mfd.Value.Hide();
					mfd.Value.Close();
				}
			});
			_windowList?.Clear();
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
            if (forceReload)
            {
                cbModules.SelectedIndex = -1;
            }
            if (moduleName != (string?)cbModules?.SelectedValue)
            {
                if (cbModules != null)
                {
                    // Log available modules for debugging
                    var availableModules = ((List<IModuleDefinition>)cbModules.ItemsSource).Select(m => m.ModuleName).ToList();
                    _logger?.LogInformation("Trying to select module: " + moduleName);

                    // Find the module in the list
                    var moduleToSelect = availableModules.FirstOrDefault(m => string.Equals(m, moduleName, StringComparison.OrdinalIgnoreCase));
                    if (moduleToSelect != null)
                    {
                        cbModules.SelectedValue = moduleToSelect;
                        if (cbModules.SelectedIndex == -1)
                        {
                            IsValid = false;
                            _logger?.LogError($"Unable to find a configuration named {moduleName}");
                            throw new ArgumentOutOfRangeException(nameof(moduleName));
                        }
                        else
                        {
                            IsValid = true;
                        }
                    }
                    else
                    {
                        IsValid = false;
                        _logger?.LogError($"Module name '{moduleName}' not found in available modules.");
                        throw new ArgumentOutOfRangeException(nameof(moduleName));
                    }
                }
            }
            UpdateMenu();
        }

		/// <summary>
		/// Event for the module selection change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
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
					_selectedModule = (ModuleDefinition?)e.AddedItems[0];
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
                RestoreWindowPosition();

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
			_logger?.LogInformation($"Using appFolder: {appDataFolder}");
			var cacheFolder = Path.Combine(appDataFolder ?? "", Properties.Resources.BaseDataDirectory, "cache");
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
			MessageBox.Show(MainApp.VersionString, "Help->About");
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
        public void ReloadConfiguration(bool forceReload = false)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var mainApp = Application.Current as MainApp;
            mainApp?.ReloadConfiguration();
            _module = _startOptions?.ModuleName;
            _subModule = _startOptions?.SubModuleName;
            DestroyWindows();
            SetupWindow();
            var selectedModule = _module ?? _settings.DefaultConfiguration;
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

        public void SubscribeToModuleSwitch(MainApp app)
        {
            app.ModuleSwitchRequested += (module, subModule) =>
            {
                _logger.LogInformation($"Module switch requested: {module}, {subModule}");
                SwitchModule(module, subModule);
            };
        }

        public void SwitchModule(string moduleName, string subModuleName)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _startOptions.ModuleName = moduleName;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            _startOptions.SubModuleName = subModuleName;
            ReloadConfiguration(true);
        }
	}
}
