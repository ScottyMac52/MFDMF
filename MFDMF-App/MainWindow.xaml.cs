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
		/// Configuration for all modules <see cref="IMFDMFDefinition"/>
		/// </summary>
		protected IMFDMFDefinition Configuration { get; private set; }
		/// <summary>
		/// List of all created <see cref="ConfigurationWindow"/> Windows
		/// </summary>
		protected SortedList<string, ConfigurationWindow> WindowList { get; private set; }
		/// <summary>
		/// The list of available modules
		/// </summary>
		private List<ModuleDefinition> _availableModules;
		/// <summary>
		/// Currently selected Module
		/// </summary>
		protected IModuleDefinition SelectedModule { get; set; }
		/// <summary>
		/// The name of the module that was passed in
		/// </summary>
		protected string PassedModule { get; private set; }
		/// <summary>
		/// The name of the sub-module that was passed
		/// </summary>
		protected string PassedSubModule { get; private set; }
		/// <summary>
		/// List of Test Pattern definitions
		/// </summary>
		private List<TestPatternDefinition> TestPatternDefinitions { get; set; }


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
			WindowList = new SortedList<string, ConfigurationWindow>();
			_startOptions = (StartOptions) ((MainApp)Application.Current).Host.Services.GetService(typeof(StartOptions));
		}

		#endregion Constructor
		
		#region Modules and SubModule processing

		/// <summary>
		/// Creates all the windows for the <see cref="ConfigurationDefinition"/> definitions
		/// </summary>
		private void CreateWindows()
		{
			var displayDefinitions = _displayConfigurationService.LoadDisplays();

			if ((TestPatternDefinitions?.Count ?? 0) > 0)
			{
				_logger.LogInformation($"Creating TestPattern Definitions for {TestPatternDefinitions?.Count ?? 0} Configurations");
				TestPatternDefinitions.ForEach(testPattern =>
				{
					var configWindow = new ConfigurationWindow(_loggerFactory, displayDefinitions, _settings, true)
					{
						Configuration = testPattern,
						FilePath = testPattern.FilePath,
						SubConfigurationName = PassedSubModule
					};
					configWindow.LoadImageFromBytes(testPattern.ImageBytes);
					configWindow.Show();
					if (configWindow.IsWindowLoaded)
					{
						WindowList.Add(testPattern.Name, configWindow);
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
				_logger?.LogDebug($"Creating configuration {SelectedModule?.DisplayName}");
				SelectedModule?.Configurations?.ForEach(config =>
				{
					if (config?.Enabled ?? false)
					{
						_logger?.LogInformation($"Creating {config.ToReadableString()}");
						var configWindow = new ConfigurationWindow(_loggerFactory, displayDefinitions, _settings)
						{
							Configuration = config,
							FilePath = config.FilePath,
							SubConfigurationName = PassedSubModule
						};
						configWindow.Show();
						if (configWindow.IsWindowLoaded)
						{
							WindowList.Add(config.Name, configWindow);
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
			WindowList.ToList().ForEach(mfd =>
			{
				if (mfd.Value.IsLoaded)
				{
					mfd.Value.Hide();
					mfd.Value.Close();
				}
			});
			WindowList.Clear();
			_logger?.LogInformation(Properties.Resources.WindowListCleared);
		}

		/// <summary>
		/// Sets up the main window
		/// </summary>
		private void SetupWindow()
		{
			//WindowList = new SortedList<string, AuxWindow>();
			var moduleList = Configuration?.Modules;
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
			SelectedModule = _availableModules.FirstOrDefault(am => am.ModuleName == moduleName);
			return SelectedModule != null;
		}


		/// <summary>
		/// Used to change the selected module 
		/// </summary>
		/// <param name="moduleName"></param>
		public void ChangeSelectedModule(string moduleName)
		{
			if (moduleName != (string)cbModules?.SelectedValue)
			{
				cbModules.SelectedValue = moduleName;
			}
		}

		/// <summary>
		/// Changes the selected sub-module
		/// </summary>
		/// <param name="subModeSpecified"></param>
		public void ChangeSelectedSubModule(string subModeSpecified)
		{
			PassedSubModule = subModeSpecified;
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
			TestPatternDefinitions = null;
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

		private void CbModules_Loaded(object sender, RoutedEventArgs e)
		{

			if (!string.IsNullOrEmpty(Configuration?.DefaultConfig) || !string.IsNullOrEmpty(PassedModule))
			{
				if (string.IsNullOrEmpty(PassedModule))
				{
					_logger?.LogInformation($"Loading the default configuration {Configuration?.DefaultConfig}");
				}
				else
				{
					_logger?.LogInformation($"Loading the requested configuration {PassedModule}");
				}
				var selectedModule = PassedModule ?? Configuration?.DefaultConfig;

				var selectedMod = _availableModules.FirstOrDefault(am => am.ModuleName.Equals(selectedModule, StringComparison.InvariantCulture));
				if(selectedMod == null)
				{
					_logger?.LogError($"Unable to find a module named {selectedModule} in the configuration. Selecting the default module {Configuration?.DefaultConfig}");
					selectedModule = Configuration?.DefaultConfig;
				}
				cbModules.SelectedValue = selectedModule;
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
			PassedModule = _startOptions?.ModuleName;
			PassedSubModule = _startOptions?.SubModuleName;
			ReloadConfiguration();
			SetupWindow();
		}

		#endregion Window events

		#region Menu Item processing

		private void FileMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Clears the cache of all PNG files
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClearCache_Click(object sender, RoutedEventArgs e)
		{
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
			ReloadConfiguration();
		}

		/// <summary>
		/// Used to always load the configuration
		/// </summary>
		private void ReloadConfiguration()
		{
			var module = (string)cbModules.SelectedValue;
			DestroyWindows();
			Configuration = _loadingService?.LoadConfiguration();
			_logger?.LogInformation($"Loaded configuration from {_settings.ConfigurationFile} Module: {PassedModule} SubModule: {PassedSubModule} {Configuration?.Modules?.Count ?? 0} Modules loaded");
			
			if((_settings.ModuleNames?.Count ?? 0) > 0)
			{
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
			ChangeSelectedModule(module ?? Configuration?.DefaultConfig);
		}

		private bool PreProcessModule(ModuleDefinition arg)
		{
			arg?.PreProcessModule(Configuration);
			Configuration?.Modules?.Add(arg);
			return true;
		}
		
		/// <summary>
		/// Generates and displays the configured Test Pattern
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Generate_Pattern_Click(object sender, RoutedEventArgs e)
		{
			TestPatternDefinitions = new List<TestPatternDefinition>();
			// Evaluate choice
			var displays = _displayConfigurationService.LoadDisplays();

			displays.ForEach(display =>
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
					TestPatternDefinitions.Add(testPattern);
				}
			});

			DestroyWindows();
			CreateWindows();
		}


		#endregion Menu Item processing

	}
}
