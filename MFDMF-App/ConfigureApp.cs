﻿using Microsoft.Extensions.Configuration;
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

        private static string SavedGamesFolder => GetSpecialFolder("{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}", Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "SavedGames"));

        /// <summary>
        /// Configuration method for all injected dependencies
        /// </summary>
        /// <param name="addtionalServices"><see cref="Action{IServiceCollection, IConfiguration}"/> used to invoke custom dependency injection</param>
        /// <returns></returns>
        internal static IHost Configure(Action<IServiceCollection, IConfiguration>? addtionalServices = null)
		{
			try
			{
				return Host.CreateDefaultBuilder()
					.ConfigureAppConfiguration((context, builder) =>
					{
						builder.Sources.Clear();
						builder.AddJsonFile("appsettings.json", false, true);
						builder.AddEnvironmentVariables();
					})
					.ConfigureServices((context, services) =>
					{
						//  Configure additional services and configurations
						addtionalServices?.Invoke(services, context.Configuration);
					})
					.UseSerilog((hostBuilderContext, loggerConfiguration) =>
					{
						var logFile = $"{SavedGamesFolder}\\Vyper Industries\\MFDMF\\Logs\\status";
						loggerConfiguration.MinimumLevel.Warning();
						loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMFApp", LogEventLevel.Information);
						loggerConfiguration.MinimumLevel.Override("MFDMF_Services.Configuration", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMF_Services.Displays", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMF_Services.ConfigurationProvider", LogEventLevel.Warning);
						loggerConfiguration.Enrich.FromLogContext();
						loggerConfiguration.WriteTo.RollingFile(logFile + "-{Date}.log", LogEventLevel.Verbose, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm}] [{Level}] [{SourceContext}] [{Message}]{NewLine}{Exception}");
					})
					.Build();
			}
			catch (Exception)
			{
				throw;
			}		
		}

        private static string GetSpecialFolder(string folderName, string defaultValue = null)
        {
            var regKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
            var regKeyValue = folderName;
#pragma warning disable CA1416 // Validate platform compatibility
            return (string)Registry.GetValue(regKey, regKeyValue, defaultValue);
#pragma warning restore CA1416 // Validate platform compatibility
        }

    }
}