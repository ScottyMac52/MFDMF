using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace MFDMFApp
{
	internal class ConfigureApp
	{
		/// <summary>
		/// Configuration method for all injected dependencies
		/// </summary>
		/// <param name="addtionalServices"><see cref="Action{IServiceCollection, IConfiguration}"/> used to invoke custom dependency injection</param>
		/// <returns></returns>
		internal static IHost Configure(Action<IServiceCollection, IConfiguration> addtionalServices = null)
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
						var appFolder = MainApp.AppDataFolder;
						var logFile = Path.Combine(appFolder, $"{Properties.Resources.BaseDataDirectory}Logs\\status");
						loggerConfiguration.MinimumLevel.Warning();
						loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMFApp", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMF_Services", LogEventLevel.Warning);
						loggerConfiguration.MinimumLevel.Override("MFDMF_Services.Configuration", LogEventLevel.Information);
						loggerConfiguration.Enrich.FromLogContext();
						loggerConfiguration.WriteTo.RollingFile(logFile + "-{Date}.log", LogEventLevel.Information, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{SourceContext}] [{Message}]{NewLine}{Exception}");
					})
					.Build();
			}
			catch (Exception)
			{
				throw;
			}		
		}
	}
}