using CommandLine;
using MFDMF_Models.Models;
using MFDMF_Services.Configuration;
using MFDMF_Services.Displays;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace MFDMFApp
{
	public class MainApp : Application
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public IHost Host { get; }

		/// <summary>
		/// Application Ctor
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="IOException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		public MainApp(string[] args)
		{
			Host = ConfigureApp.Configure((services, configuration) =>
			{
				services.AddScoped<IConfigurationLoadingService, ConfigurationLoadingService>();
				services.AddScoped<IDisplayConfigurationService, DisplayConfigurationService>();
				services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
				services.Configure<AppSettings>(options =>
				{
					var modulesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Modules");
					options.ModuleNames = Directory.GetFiles(modulesDirectory, "*.json", SearchOption.AllDirectories)?.ToList();
				});
				services.AddSingleton(GetStartOptions(args));
				services.AddSingleton<MainWindow>();
			});

			_loggerFactory = Host.Services.GetRequiredService<ILoggerFactory>();
			_logger = _loggerFactory.CreateLogger(typeof(MainApp));
			_logger?.LogInformation($"Starting {GetVersionString()}");
			DispatcherUnhandledException += MainApp_DispatcherUnhandledException;
		}

		/// <summary>
		/// Loads the start options 
		/// </summary>
		/// <param name="args">Startup args passed</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		protected static StartOptions GetStartOptions(string[] args)
		{
			StartOptions startOptions = null;
			Parser.Default.ParseArguments<StartOptions>(args).WithParsed(o =>
			{
				startOptions = new StartOptions()
				{
					ModuleName = o.ModuleName,
					SubModuleName = o.SubModuleName
				};
			});
			return startOptions;
		}

		/// <summary>
		/// Async Startup handler that gets the Singleton <see cref="MainWindow"/> and shows it
		/// </summary>
		/// <param name="e"></param>
		/// <exception cref="InvalidOperationException"></exception>
		protected override async void OnStartup(StartupEventArgs e)
		{
			await (Host?.StartAsync()).ConfigureAwait(true);
			var mainWindow = Host?.Services?.GetRequiredService<MainWindow>();
			_logger?.LogInformation(MFDMFApp.Properties.Resources.StartingMain);
			mainWindow?.Show();
			base.OnStartup(e);
		}

		/// <summary>
		/// Async Exit handler
		/// </summary>
		/// <param name="e"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref=""
		protected override async void OnExit(ExitEventArgs e)
		{
			_logger?.LogInformation($"Shutting down with exit code {e?.ApplicationExitCode ?? 0}");
			using (Host)
			{
				await Host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
			}
			base.OnExit(e);
		}

		/// <summary>
		/// Gets the Name and version of the application
		/// </summary>
		public static string VersionString => GetVersionString();

		/// <summary>
		/// Gets the version information from the assembly
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		private static string GetVersionString()
		{
			var exeAssem = Assembly.GetExecutingAssembly();
			var productAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
			var copyrightAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
			var companyAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
			var versionAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			return $"{productAttribute.Product} Copyright {companyAttribute.Company} {copyrightAttribute.Copyright} version {versionAttribute.InformationalVersion}";
		}

		/// <summary>
		/// Handle dispatched exceptions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			var errorMessage = $"{sender.GetType().FullName} reports unexpected exception has occurred. The exception details have been logged and the message is {e.Exception.Message}";
			MessageBox.Show(errorMessage, MFDMFApp.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
			_logger?.LogError($"{errorMessage}, {e.Exception}");
			Shutdown(-1);
		}


	}
}
