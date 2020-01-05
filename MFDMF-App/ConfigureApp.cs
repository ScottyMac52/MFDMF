using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace MFDMF_App
{
    internal class ConfigureApp
	{
		internal static IHost Configure(Action<IServiceCollection, IConfiguration> addtionalServices = null)
		{
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.local.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context.Configuration, services, addtionalServices);
                })
                .ConfigureLogging(configure =>
                {
                    configure.AddConsole(options =>
                    {
                        options.DisableColors = false;
                    });
                })
                .Build();
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services, Action<IServiceCollection, IConfiguration> addtionalServices)
        {
            services.AddSingleton(CreateLoggerFactory);
            addtionalServices?.Invoke(services, configuration);
        }

        private static ILoggerFactory CreateLoggerFactory(IServiceProvider arg)
        {
            return LoggerFactory.Create(configure =>
            {
                configure.AddLog4Net();
                configure.AddConsole();
                configure.AddFilter("Microsoft", LogLevel.Warning);
            });
        }
    }
}