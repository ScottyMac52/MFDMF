using MFDMF_Models.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
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

		string RulerName { get; set; }

		/// <summary>
		/// List of sub configurations
		/// </summary>
		List<ConfigurationDefinition> SubConfigurations { get; set; }

		/// <summary>
		/// If true then the superimposed bitmaps are solid 
		/// </summary>
		bool? MakeOpaque { get; set; }

		bool CheckForActiveSelectedSubConfiguration(List<string> specifiedSubConfigs);

		string GetImagePrefix(List<string> selectedSubMods);
	}
}