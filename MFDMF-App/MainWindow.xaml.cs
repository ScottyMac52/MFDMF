using log4net;
using MFDMF_Models;
using MFDMF_Models.Comparer;
using MFDMF_Models.Interfaces;
using MFDMF_Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MFDMF_App
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly AppSettings _settings;
		private readonly IConfigurationLoadingService _loadingService;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<MainWindow> _logger;
		public IMFDMFDefinition Configuration { get; private set; }
		//public SortedList<string, AuxWindow> WindowList { get; private set; }

		/// <summary>
		/// The list of available modules
		/// </summary>
		protected List<ModuleDefinition> AvailableModules { get; set; }

		/// <summary>
		/// Currently selected Module
		/// </summary>
		protected ModuleDefinition SelectedModule { get; set; }

		/// <summary>
		/// The name of the module that was passed in
		/// </summary>
		public string PassedModule { get; internal set; }

		/// <summary>
		/// The name of the sub-module that was passed
		/// </summary>
		public string PassedSubModule { get; internal set; }

		public MainWindow(IConfigurationLoadingService loadingService, IOptions<AppSettings> settings, ILoggerFactory loggerFactory)
		{
			InitializeComponent();
			_settings = settings.Value;
			_loadingService = loadingService;
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory.CreateLogger<MainWindow>();
		}

		#region Public methods

		/// <summary>
		/// Sets up the main window
		/// </summary>
		public void SetupWindow()
		{
			//WindowList = new SortedList<string, AuxWindow>();
			var moduleList = Configuration?.Modules;
			moduleList?.Sort(new ModuleDefinitionComparer());
			AvailableModules = moduleList;
			cbModules.ItemsSource = moduleList;
			cbModules.DisplayMemberPath = "DisplayName";
			cbModules.SelectedValuePath = "ModuleName";
		}

		/// <summary>
		/// Gets the specified Definition
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public bool GetSelectedDefinition(string moduleName)
		{
			if (string.IsNullOrEmpty(moduleName))
			{
				return false;
			}
			_logger?.LogInformation($"Configuration requested for {moduleName}");
			SelectedModule = AvailableModules.FirstOrDefault(am => am.ModuleName == moduleName);
			return SelectedModule != null;
		}


		public void CreateWindows()
		{
			_logger?.LogDebug($"Creating configuration {SelectedModule?.DisplayName}");
			SelectedModule?.Configurations?.ForEach(config =>
			{
				_logger?.LogInformation($"Creating {config.ToReadableString()}");
				/*
				var newAuxWindow = new AuxWindow()
				{
					Logger = Logger,
					Configuration = config,
					FilePath = Config.FilePath,
					SubConfigurationName = PassedSubModule
				};
				newAuxWindow.Show();
				if (newAuxWindow.IsWindowLoaded)
				{
					WindowList.Add(config.Name, newAuxWindow);
					newAuxWindow.Visibility = Visibility.Visible;
				}
				else
				{
					newAuxWindow?.Close();
				}
				*/
			});
		}

		public void DestroyWindows()
		{
			/*
			WindowList.ToList().ForEach(mfd =>
			{
				if (mfd.Value.IsLoaded)
				{
					mfd.Value.Hide();
					mfd.Value.Close();
				}
			});

			WindowList.Clear();
			*/
			_logger?.LogInformation($"Window list cleared.");
		}

		#endregion Public methods

		#region Modules and SubModule processing

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
				_logger?.LogError($"Not able to determine selected module", ioorx);
			}
			catch(Exception ex)
			{
				_logger?.LogError($"Unexpected exception has occurred, not able to determine selected module", ex);
			}
		}

		private void ProcessChangedModule(string moduleName)
		{
			if (GetSelectedDefinition(moduleName))
			{
				DestroyWindows();
				try
				{
					CreateWindows();
					_logger?.LogInformation($"Module loaded {moduleName}.");
				}
				catch (IndexOutOfRangeException ioorx)
				{
					_logger?.LogError($"Not able to determine selected module", ioorx);
				}
			}
			else
			{
				_logger?.LogError($"{moduleName} does not exist as a module in the current configuration.");
			}

		}

		private void CbModules_Loaded(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(Configuration.DefaultConfig) || !string.IsNullOrEmpty(PassedModule))
			{
				if (string.IsNullOrEmpty(PassedModule))
				{
					_logger?.LogInformation($"Loading the default configuration {Configuration.DefaultConfig}...");
				}
				else
				{
					_logger?.LogInformation($"Loading the requested configuration {PassedModule}...");
				}
				var selectedModule = PassedModule ?? Configuration.DefaultConfig;
				cbModules.SelectedValue = selectedModule;
			}
		}

		#endregion Modules and SubModule processing

		#region Window events

		private void Window_Closed(object sender, EventArgs e)
		{
			_logger?.LogInformation($"MainWindow Is Closed.");
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_logger?.LogInformation("Closing the Windows...");
			DestroyWindows();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_logger?.LogInformation($"Loading configuration from {_settings.ConfigurationFile}");
			Configuration = _loadingService?.LoadConfiguration();
			_logger?.LogInformation($"{Configuration?.Modules?.Count ?? 0} Modules loaded");
			SetupWindow();
		}

		#endregion Window events
		
		#region Menu Item processing

		private void FileMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
			App.Current.Shutdown(0);
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
			var fileList = cacheParent?.EnumerateFiles("*.*", SearchOption.AllDirectories).ToList();
			fileList?.ForEach((file) =>
			{
				try
				{
					file?.Delete();
				}
				catch (Exception ex)
				{
					_logger?.LogError($"Unable to delete cache file: {file.FullName}", ex);
				}
			});
			try
			{
				cacheParent?.Delete(true);

			}
			catch (Exception ex)
			{
				_logger?.LogError($"Unable to delete cache directory: {cacheFolder}", ex);
			}
			CreateWindows();
		}


		private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(((App)Application.Current).VersionString);
		}

		/// <summary>
		/// Menu item to relaod the current configuration
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReloadConfiguration_Click(object sender, RoutedEventArgs e)
		{
			var module = (string)cbModules.SelectedValue;
			DestroyWindows();
			Configuration = _loadingService?.LoadConfiguration();
			SetupWindow();
			ChangeSelectedModule(module);
		}

		#endregion Menu Item processing

	}
}
