namespace MFDMF_Models.Interfaces
{
	using MFDMF_Models.Models;
	using Microsoft.Extensions.Logging;
	using System.Collections.Generic;
	using System.Drawing;

	public interface IConfigurationDefinition : IReadableObject, IImagePath, INameObject, IModuleName, IDisplayGeometry, IOffsetGeometry
	{
		ILogger Logger { get; set; }

		/// <summary>
		/// The parent to this configuration
		/// </summary>
		IConfigurationDefinition Parent { get; set; }

		/// <summary>
		/// Throttle type
		/// </summary>
		string ThrottleType { get; set; }

		/// <summary>
		/// The name of the ruler
		/// </summary>
		string RulerName { get; set; }

		/// <summary>
		/// List of sub configurations
		/// </summary>
		List<ConfigurationDefinition> SubConfigurations { get; set; }
	
		/// <summary>
		/// Determines if the configuration is valid
		/// </summary>
		bool IsValid { get; }

		bool CheckForActiveSelectedSubConfiguration(List<string> specifiedSubConfigs);

		string GetImagePrefix(List<string> selectedSubMods);
	}
}