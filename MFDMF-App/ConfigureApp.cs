using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using Serilog;
using System;
using System.IO;

namespace MFDMFApp
{
    internal class ConfigureApp
    {
        private static string? SavedGamesFolder => GetSpecialFolder("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}", Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "SavedGames"));

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
                    builder.AddJsonFile(configPath, false, true);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    addtionalServices?.Invoke(services, context.Configuration);
                })
                .UseSerilog((hostBuilderContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(hostBuilderContext.Configuration) // <-- This line is critical
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