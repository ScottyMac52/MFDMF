using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
	public interface IConfigurationDefinition : IReadableObject, IImagePath, INameObject, IModuleName, IDisplayGeometry, IOffsetGeometry
	{
		/// <summary>
		/// The parent to this configuration
		/// </summary>
		IConfigurationDefinition Parent { get; set; }

		List<ConfigurationDefinition> SubConfigurations { get; set; }

		/// <summary>
		/// If true then the superimposed bitmaps are solid 
		/// </summary>
		bool? MakeOpaque { get; set; }

		/// <summary>
		/// If true then the configuration is centered to it's parent
		/// </summary>
		bool? Center { get; set; }
	}
}