namespace MFDMFApp.Extensions
{
	using Microsoft.Extensions.Logging;
	using System;

	public static class LoggingExtensions
	{
		public static void LogSystemStart(this ILogger logger) => logger?.LogWarning(Properties.Resources.StartingMain);

		public static void LogSystemShutdown(this ILogger logger, int exitCode) => logger?.LogWarning($"{Properties.Resources.ApplicationExit} exitCode: {exitCode}");

		public static void LogModules(this ILogger logger, string modulesLoaded) => logger.LogDebug(modulesLoaded);

		public static void LogUnexpectedException(this ILogger logger, Exception ex, string errorMessage) => logger?.LogCritical(ex, errorMessage);
	}
}
