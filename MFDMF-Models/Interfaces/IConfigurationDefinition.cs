namespace MFDMF_Models.Interfaces
{
	using MFDMF_Models.Models;
	using Microsoft.Extensions.Logging;
	using System.Collections.Generic;

	public interface IConfigurationDefinition : IReadableObject, IImagePath, INameObject, IModuleName, IDisplayGeometry, IOffsetGeometry
	{
		/// <summary>
		/// Logger
		/// </summary>
		ILogger Logger { get; set; }

		/// <summary>
		/// The display that is used to setup the configuration, can be null 
		/// </summary>
		string DisplayName { get; set; }

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

		/// <summary>
		/// Used to verify if a sub-configuration is selected or always active
		/// </summary>
		/// <param name="specifiedSubConfigs"></param>
		/// <returns></returns>
		bool CheckForActiveSelectedSubConfiguration(List<string> specifiedSubConfigs);

		/// <summary>
		/// Single way to get the image prefix
		/// </summary>
		/// <param name="selectedSubMods"></param>
		/// <returns></returns>
		string GetImagePrefix(List<string> selectedSubMods);
	}
}