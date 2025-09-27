using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using Serilog;
using Serilog.Settings.Configuration; // <-- needed for ConfigurationReaderOptions
using System;
using System.IO;
using System.Linq;

namespace MFDMFApp
{
    internal class ConfigureApp
    {
        private static string? SavedGamesFolder =>
            GetSpecialFolder("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}",
                Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "SavedGames"));

        public static string HOME_PATH => $"{SavedGamesFolder}\\Vyper Industries\\MFDMF";

        internal static IHost Configure(Action<IServiceCollection, IConfiguration>? addtionalServices = null)
        {
            var savedGamesPath = HOME_PATH;
            var logDir = Path.Combine(savedGamesPath, "Logs");
            var logPath = Path.Combine(logDir, "status-.log"); // Serilog will append date automatically
            Directory.CreateDirectory(logDir);

            var configPath = Path.Combine(savedGamesPath, "Config", "appsettings.json");

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.Sources.Clear();

                    // Load external JSON if present; don't crash if it's missing.
                    if (File.Exists(configPath))
                        builder.AddJsonFile(configPath, optional: false, reloadOnChange: true);

                    // Allow overrides via env vars (e.g., Serilog__MinimumLevel__Default, etc.)
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    addtionalServices?.Invoke(services, context.Configuration);
                })
                .UseSerilog((hostBuilderContext, loggerConfiguration) =>
                {
                    // In single-file, Serilog can't probe for sinks. Provide the sink assembly explicitly.
                    // We know we use the File sink (Serilog.Sinks.File), which exposes FileLoggerConfigurationExtensions.
                    var options = new ConfigurationReaderOptions(typeof(Serilog.FileLoggerConfigurationExtensions).Assembly);

                    // If the Serilog section exists, try to read it with explicit options; otherwise skip.
                    try
                    {
                        var hasSerilogSection =
                            hostBuilderContext.Configuration.GetSection("Serilog")?.GetChildren()?.Any() == true;

                        if (hasSerilogSection)
                        {
                            loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration, options);
                        }
                    }
                    catch
                    {
                        // If config-reading fails (e.g., due to missing Using/unknown sinks), continue with code config below.
                    }

                    // Always ensure we have a file sink so logging works regardless of config.
                    loggerConfiguration
                        .Enrich.FromLogContext()
                        .WriteTo.File(
                            path: logPath,
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm}] [{Level}] [{SourceContext}] [{Message}]{NewLine}{Exception}"
                        );
                })
                .Build();

            return hostBuilder;
        }

        private static string? GetSpecialFolder(string folderName, string? defaultValue = null)
        {
            var regKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
            var regKeyValue = folderName;
#pragma warning disable CA1416 // Validate platform compatibility
            return Registry.GetValue(regKey, regKeyValue, defaultValue) as string;
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
}
