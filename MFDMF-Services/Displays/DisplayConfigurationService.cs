using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MFDMF_Services.Displays
{
	public class DisplayConfigurationService : IDisplayConfigurationService
	{
		private readonly AppSettings _settings;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger<IDisplayConfigurationService> _logger;
		
		public DisplayConfigurationService(IOptions<AppSettings> settings, ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory?.CreateLogger<DisplayConfigurationService>();
			_settings = settings.Value;
		}

		public List<DisplayDefinition> LoadDisplays()
		{
			var jsonFile = Path.Combine(Directory.GetCurrentDirectory(), _settings.DisplayConfigurationFile);
			_logger?.LogInformation($"Loading configuration from {jsonFile}");
			var fileContent = File.ReadAllText(jsonFile);
			var displayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(fileContent); 
			return displayList;
		}

		public bool SaveDisplayConfigurations(List<DisplayDefinition> displayDefinitions)
		{
			throw new NotImplementedException();
		}
	}
}
