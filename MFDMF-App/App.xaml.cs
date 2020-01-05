using MFDMF_Models;
using MFDMF_Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace MFDMF_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
	{
        public readonly IHost _host;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public App()
        {
            _host = ConfigureApp.Configure((services, configuration) =>
            {
                services.AddScoped<IConfigurationLoadingService, ConfigurationLoadingService>();
                services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
                services.AddSingleton<MainWindow>();
            });

            _loggerFactory = _host.Services.GetRequiredService<ILoggerFactory>();
            _logger = _loggerFactory.CreateLogger(typeof(App));
            _logger.LogInformation($"Starting {GetVersionString()}");
        }

        /// <summary>
        /// Gets the Name and version of the application
        /// </summary>
        public string VersionString => GetVersionString();

        private string GetVersionString()
        {
            var exeAssem = Assembly.GetExecutingAssembly();
            var productAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            var copyrightAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var companyAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            var versionAttribute = exeAssem.GetCustomAttributes().FirstOrDefault(ca => ca.GetType() == typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

            return $"{productAttribute.Product} Copyright {companyAttribute.Company} {copyrightAttribute.Copyright} version {versionAttribute.InformationalVersion}";
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            _logger?.LogInformation("Starting the main window...");
            await _host?.StartAsync();
            var mainWindow = _host?.Services?.GetRequiredService<MainWindow>();
            mainWindow?.Show();
            base.OnStartup(e);
        }

        /// <summary>
        /// Executed when the application exits.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnExit(ExitEventArgs e)
        {
            _logger?.LogInformation($"Shutting down with exit code {e.ApplicationExitCode}");
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError($"{sender?.GetType()?.Name} threw an exception: {e.Exception}");
            Shutdown(-1);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _logger?.LogInformation("Shutdown");
        }
    }
}
