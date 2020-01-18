using MFDMF_Models.Comparer;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Models.Models.TestPattern;
using MFDMF_Services.Configuration;
using MFDMF_Services.Displays;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
		#region IoC Injected fields
		private readonly AppSettings _settings;
		private readonly IConfigurationLoadingService _loadingService;
		private readonly IDisplayConfigurationService _displayConfigurationService;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<MainWindow> _logger;
		private readonly StartOptions _startOptions;
		#endregion IoC Injected fields

		#region private and protected fields and properties
		/// <summary>
		/// List of all created <see cref="ConfigurationWindow"/> Windows
		/// </summary>
		private SortedList<string, ConfigurationWindow> _windowList;
		/// <summary>
		/// The list of available modules
		/// </summary>
		private List<ModuleDefinition> _availableModules;
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
		/// List of Test Pattern definitions
		/// </summary>
		private List<TestPatternDefinition> _testPatterns;
		/// <summary>
		/// The list of modules that are currently loaded
		/// </summary>
		private List<ModuleDefinition> _modules;
		/// <summary>
		/// List of the dsiplays for the configuration
		/// </summary>
		private List<DisplayDefinition> _displays;
		#endregion private and protected fields and properties

		#region Constructor
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="loadingService"><see cref="IConfigurationLoadingService"/> loaded via Dependency Injection</param>
		/// <param name="displayConfigurationService"><see cref="IDisplayConfigurationService"/> loaded via Dependency Injection</param>
		/// <param name="settings"><see cref="AppSettings"/> loaded via Dependency Injection</param>
		/// <param name="loggerFactory"><see cref="ILoggerFactory"/> logging factory loaded via Dependency Injection</param>
		public MainWindow(IConfigurationLoadingService loadingService, IDisplayConfigurationService displayConfigurationService, IOptions<AppSettings> settings, ILoggerFactory loggerFactory)
		{
			InitializeComponent();
			_settings = settings?.Value;
			_loadingService = loadingService;
			_displayConfigurationService = displayConfigurationService;
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory.CreateLogger<MainWindow>();
			_windowList = new SortedList<string, ConfigurationWindow>();
			_startOptions = (StartOptions) ((MainApp)Application.Current).Host.Services.GetService(typeof(StartOptions));
			_modules = new List<ModuleDefinition>();
		}
		#endregion Constructor
		
		#region Modules and SubModule processing

		/// <summary>
		/// Creates all the windows for the <see cref="ConfigurationDefinition"/> definitions
		/// </summary>
		private void CreateWindows()
		{
			if ((_testPatterns?.Count ?? 0) > 0)
			{
				_logger.LogInformation($"Creating TestPattern Definitions for {_testPatterns?.Count ?? 0} Configurations");
				_testPatterns.ForEach(testPattern =>
				{
					var configWindow = new ConfigurationWindow(_loggerFactory, _displays, _settings, testPattern.ImageBytes)
					{
						Configuration = testPattern,
						FilePath = testPattern.FilePath,
						SubConfigurationName = _subModule
					};
					configWindow.Show();
					if (configWindow.IsWindowLoaded)
					{
						_windowList.Add(testPattern.Name, configWindow);
						configWindow.Visibility = Visibility.Visible;
					}
					else
					{
						configWindow?.Close();
					}
				});
			}
			else
			{
				_logger?.LogDebug($"Creating configuration {_selectedModule?.DisplayName}");
				_selectedModule?.Configurations?.ForEach(config =>
				{
					if (config?.Enabled ?? false)
					{
						_logger?.LogInformation($"Creating {config.ToReadableString()}");
						var configWindow = new ConfigurationWindow(_loggerFactory, _displays, _settings)
						{
							Configuration = config,
							FilePath = config.FilePath,
							SubConfigurationName = _subModule
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
			}
		}

		/// <summary>
		/// Destroys all active <see cref="ConfigurationWindow"/>
		/// </summary>
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
		private void SetupWindow()
		{
			//WindowList = new SortedList<string, AuxWindow>();
			var moduleList = _modules;
			moduleList?.Sort(new ModuleDefinitionComparer());
			_availableModules = moduleList;
			cbModules.ItemsSource = moduleList;
			cbModules.DisplayMemberPath = "DisplayName";
			cbModules.SelectedValuePath = "ModuleName";
		}

		/// <summary>
		/// Gets the specified Definition
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		private bool GetSelectedDefinition(string moduleName)
		{
			if (string.IsNullOrEmpty(moduleName))
			{
				return false;
			}
			_logger?.LogInformation($"Configuration requested for {moduleName}");
			_selectedModule = _availableModules.FirstOrDefault(am => am.ModuleName == moduleName);
			return _selectedModule != null;
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
				_logger?.LogError($"Unable to find a configuration name {moduleName}");
				throw new ArgumentOutOfRangeException(nameof(moduleName));
			}
		}

		/// <summary>
		/// Changes the selected sub-module
		/// </summary>
		/// <param name="subModeSpecified"></param>
		public void ChangeSelectedSubModule(string subModeSpecified)
		{
			_subModule = subModeSpecified;
			DestroyWindows();
			CreateWindows();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				var selectedModule = e.AddedItems.Count > 0 ? (ModuleDefinition)e.AddedItems[0] : e.RemovedItems.Count > 0 ? (ModuleDefinition)e.RemovedItems[0] : null;
				_logger?.LogInformation($"Module selected {selectedModule.ToReadableString()}");
				ProcessChangedModule(selectedModule.ModuleName);
			}
			catch (IndexOutOfRangeException ioorx)
			{
				_logger?.LogError($"Not able to determine selected module {ioorx}");
			}
		}

		private void ProcessChangedModule(string moduleName)
		{
			_testPatterns = null;
			if (GetSelectedDefinition(moduleName))
			{
				DestroyWindows();
				try
				{
					CreateWindows();
					_logger?.LogInformation($"Module loaded {moduleName}");
				}
				catch (IndexOutOfRangeException ioorx)
				{
					_logger?.LogError($"Not able to determine selected module {ioorx}");
				}
			}
			else
			{
				_logger?.LogError($"{moduleName} does not exist as a module in the current configuration");
			}

		}

		#endregion Modules and SubModule processing

		#region Window events

		private void Window_Closed(object sender, EventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.MainWindowClosed);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.MainWindowClosing);
			DestroyWindows();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_displays = _displayConfigurationService.LoadDisplays();
			_module = _startOptions?.ModuleName;
			_subModule = _startOptions?.SubModuleName;
			ReloadConfiguration();
			SetupWindow();
		}

		#endregion Window events

		#region Menu Item processing

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
		private void ClearCache_Click(object sender, RoutedEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.UserRequestedCacheClear);
			DestroyWindows();
			var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Vyper Industries\\MFDMF\\cache\\");
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
					throw;
				}
			});
			CreateWindows();
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
		private void ReloadConfiguration(bool forceReload = false)
		{
			var module = (string)cbModules.SelectedValue;
			DestroyWindows();
			
			if((_settings.ModuleNames?.Count ?? 0) > 0)
			{
				_modules?.Clear();
				_settings.ModuleNames.ForEach(mf =>
				{
					try
					{
						var fileToLoad = Path.Combine(Directory.GetCurrentDirectory(), mf);
						var modulesToAdd = _loadingService.LoadModulesConfigurationFile(fileToLoad);
						modulesToAdd.All(PreProcessModule);
					}
					catch(JsonException jsonex)
					{
						_logger?.LogError($"Unable to load file {mf}. Exception: {jsonex}");
					}
				});
			}
			SetupWindow();
			var selectedModule = module ??= _module ??= _settings.DefaultConfiguration;
			_logger.LogInformation($"Loading module {selectedModule}");
			ChangeSelectedModule(selectedModule, forceReload);
		}

		private bool PreProcessModule(ModuleDefinition arg)
		{
			if(arg == null)
			{
				return false;
			}

			arg.Enabled ??= true;
			arg.FilePath ??= _settings.FilePath;

			arg?.Configurations?.ForEach(config =>
			{
				config.ModuleName ??= arg.ModuleName;
				config.FilePath ??= arg?.FilePath;
				config.FileName ??= arg.FileName;
				config.Enabled ??= arg.Enabled ??= true;

				config?.SubConfigurations?.ForEach(subConfig =>
				{
					subConfig.ModuleName ??= config?.ModuleName;
					subConfig.FilePath ??= config?.FilePath;
					subConfig.FileName ??= config?.FileName;
					subConfig.Enabled ??= config?.Enabled;
					subConfig.Opacity ??= config.Opacity;
				});
			});

			_modules?.Add(arg);
			return true;
		}
		
		/// <summary>
		/// Generates and displays the configured Test Pattern
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Generate_Pattern_Click(object sender, RoutedEventArgs e)
		{
			_logger?.LogInformation(Properties.Resources.UserRequestedTestPattern);
			_testPatterns = new List<TestPatternDefinition>();

			_displays.ForEach(display =>
			{
				// Find the Test Pattern for the current display
				var testPattern = _settings.PatternList?.FirstOrDefault(pl => (pl.Name?.Equals(display.Name, StringComparison.InvariantCulture) ?? false));

				if (testPattern != null)
				{
					var testImage = _settings.ImageList.FirstOrDefault(il => (il.Name?.Equals(testPattern.Pattern, StringComparison.InvariantCulture) ?? false));
					testPattern.Enabled = true;
					testPattern.Width = display.Width;
					testPattern.Height = display.Height;
					testPattern.XOffsetFinish = testImage.Width;
					testPattern.YOffsetFinish = testImage.Height;
					byte[] imageBytes = null;
					switch (testImage?.Name)
					{
						case "Color":
							imageBytes = Properties.Resources.Color;
							break;
						case "ConvergenceGrid":
							imageBytes = Properties.Resources.ConvergenceGrid;
							break;
						case "IndianHead":
							imageBytes = Properties.Resources.IndianHead;
							break;
						case "MaxRes":
							imageBytes = Properties.Resources.MaxRes;
							break;
						case "TV":
							imageBytes = Properties.Resources.TV;
							break;
						default:
							break;
					}
					testPattern.ImageBytes = imageBytes;
					_logger?.LogInformation($"Created Test Pattern: {testPattern}");
					_testPatterns.Add(testPattern);
				}
			});

			DestroyWindows();
			CreateWindows();
		}

		#endregion Menu Item processing
	}
}
