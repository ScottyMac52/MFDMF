using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace MFDMFApp
{
	internal class ConfigureApp
	{

        private static string? SavedGamesFolder => GetSpecialFolder("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}", Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "SavedGames"));

		/// <summary>
		/// The Home path 
		/// </summary>
		public static string? HOME_PATH => $"{SavedGamesFolder}\\Vyper Industries\\MFDMF";

        /// <summary>
        /// Configuration method for all injected dependencies
        /// </summary>
        /// <param name="addtionalServices"><see cref="Action{IServiceCollection, IConfiguration}"/> used to invoke custom dependency injection</param>
        /// <returns></returns>
        internal static IHost Configure(Action<IServiceCollection, IConfiguration>? addtionalServices = null)
		{
			try
			{
                var configPath = Path.Combine($"{HOME_PATH}", "Config", "appsettings.json");
                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        builder.Sources.Clear();
                        builder.AddJsonFile(configPath, false, true);
                        builder.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        //  Configure additional services and configurations
                        addtionalServices?.Invoke(services, context.Configuration);
                    })
                    .UseSerilog((hostBuilderContext, loggerConfiguration) =>
                    {
                        loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);
                    })
                    .Build();
                return hostBuilder;
            }
			catch (Exception)
			{
				throw;
			}		
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