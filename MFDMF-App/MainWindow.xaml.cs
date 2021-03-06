﻿using MFDMF_Models.Comparer;
using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
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
		/// The list of modules that are currently loaded
		/// </summary>
		private List<IModuleDefinition> _modules;
		/// <summary>
		/// List of the dsiplays for the configuration
		/// </summary>
		private List<IDisplayDefinition> _displays;
		#endregion Private fields

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
			_modules = new List<IModuleDefinition>();
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
			_logger?.LogDebug($"Creating configuration {_selectedModule?.DisplayName}");
			_selectedModule?.Configurations?.ForEach(config =>
			{
				if (config?.Enabled ?? false)
				{
					var configWindow = new ConfigurationWindow(_loggerFactory, _displays, _settings)
					{
						Configuration = config,
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
			_logger?.LogInformation($"Module {_selectedModule.DisplayName}: SubModule(s): {_subModule} loaded in {watch.ElapsedMilliseconds} milliseconds");

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
			var moduleList = _modules;
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
				_logger?.LogError($"Unable to find a configuration name {moduleName}");
				throw new ArgumentOutOfRangeException(nameof(moduleName));
			}
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
				_selectedModule = e.AddedItems.Count > 0 ? (ModuleDefinition)e.AddedItems[0] : e.RemovedItems.Count > 0 ? (ModuleDefinition)e.RemovedItems[0] : null;
				_logger?.LogInformation($"Module selected {_selectedModule?.ToReadableString()}");
				ProcessChangedModule(_selectedModule);
			}
			catch (IndexOutOfRangeException ioorx)
			{
				_logger?.LogError($"Not able to determine selected module {ioorx}");
			}
		}

		/// <summary>
		/// Processes the selection change of the module
		/// </summary>
		/// <param name="module"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private void ProcessChangedModule(IModuleDefinition module)
		{
			DestroyWindows();
			try
			{
				CreateWindows();
				_logger?.LogInformation($"Module loaded {module}");
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
			var watch = System.Diagnostics.Stopwatch.StartNew();
			ReloadConfiguration();
			SetupWindow();
			watch.Stop();
			_logger?.LogInformation($"Main window loaded in: {watch.ElapsedMilliseconds} milliseconds");
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
			var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Properties.Resources.BaseDataDirectory}cache\\");
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
			watch.Stop();
			_logger?.LogInformation($"Clear cache took: {watch.ElapsedMilliseconds} milliseconds");
			ReloadConfiguration(true);
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
			_displays = _displayConfigurationService.LoadDisplays();
			_module = _startOptions?.ModuleName;
			_subModule = _startOptions?.SubModuleName;
			var module = (string)cbModules?.SelectedValue;
			DestroyWindows();

			if ((_settings.ModuleNames?.Count ?? 0) > 0)
			{
				_modules?.Clear();
				_settings.ModuleNames.ForEach(mf =>
				{
					try
					{
						var fileToLoad = Path.Combine(Directory.GetCurrentDirectory(), mf);
						var modulesToAdd = _loadingService.LoadModulesConfigurationFile(fileToLoad, _displays);
						_modules?.AddRange(modulesToAdd);
					}
					catch(JsonException jsonex)
					{
						_logger?.LogError($"Unable to load file {mf}. Exception: {jsonex}");
						throw;
					}
				});
			}
			SetupWindow();
			var selectedModule = module ??= _module ??= _settings.DefaultConfiguration;
			_logger?.LogInformation($"Loading module {selectedModule}");
			if (!string.IsNullOrEmpty(selectedModule))
			{
				ChangeSelectedModule(selectedModule, forceReload);
			}
			watch.Stop();
			_logger?.LogInformation($"Reloaded configuration in: {watch.ElapsedMilliseconds} milliseconds");
		}

		#endregion Menu Item processing
	}
}
