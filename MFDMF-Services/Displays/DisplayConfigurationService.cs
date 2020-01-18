using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MFDMF_Services.Displays
{
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
		public List<DisplayDefinition> LoadDisplays()
		{
			var jsonFile = Path.Combine(Directory.GetCurrentDirectory(), _settings.DisplayConfigurationFile);
			_logger?.LogInformation($"Loading configuration from {jsonFile}");
			var fileContent = File.ReadAllText(jsonFile);
			var displayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(fileContent); 
			return displayList;
		}

		#endregion Public methods

	}
}
