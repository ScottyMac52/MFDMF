namespace MFDMF_Services.Displays
{
	using MFDMF_Models.Interfaces;
	using MFDMF_Models.Models;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;

	/// <summary>
	/// Service that loads the display configurations
	/// </summary>
	public class DisplayConfigurationService : IDisplayConfigurationService
	{
		#region Private readonly properties
		
		private readonly AppSettings _settings;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<IDisplayConfigurationService> _logger;

		#endregion Private readonly properties

		#region Ctor

		/// <summary>
		/// Ctor with injected IoC parameters
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="loggerFactory"></param>
		public DisplayConfigurationService(IOptions<AppSettings> settings, ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory?.CreateLogger<DisplayConfigurationService>();
			_settings = settings.Value;
		}

		#endregion Ctor

		#region Public methods

		/// <summary>
		/// Loads the display configuration for from the file specified in <see cref="AppSettings"/> property DisplayConfigurationFile
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<IDisplayDefinition>> LoadDisplaysAsync()
		{
			var returnList = new List<IDisplayDefinition>();
			var jsonFile = Environment.ExpandEnvironmentVariables(_settings.DisplayConfigurationFile);
			_logger?.LogInformation($"Loading configuration from {jsonFile}");
			var fileContent = await File.ReadAllTextAsync(jsonFile);
			var displayList = JsonConvert.DeserializeObject<IEnumerable<DisplayDefinition>>(fileContent);
			returnList.AddRange(displayList);
			return returnList;
		}

		#endregion Public methods

	}
}
