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
			var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Vyper Industries\\MFDMF\\Logs\\status");

			return Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration((context, builder) =>
				{
					builder.AddJsonFile("appsettings.local.json", optional: true);
				})
				.ConfigureServices((context, services) =>
				{
					//  Configure additional services and configurations
					addtionalServices?.Invoke(services, context.Configuration);
				})
				.UseSerilog((hostBuilderContext, loggerConfiguration) =>
				{
					loggerConfiguration.MinimumLevel.Debug();
					loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
					loggerConfiguration.MinimumLevel.Override("MFDMF-App", LogEventLevel.Warning);
					loggerConfiguration.Enrich.FromLogContext();
					loggerConfiguration.WriteTo.Console();
					loggerConfiguration.WriteTo.RollingFile(logFile + "-{Date}.log", LogEventLevel.Information, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{SourceContext}] [{Message}]{NewLine}{Exception}");
				})
				.Build();
		}
	}
}