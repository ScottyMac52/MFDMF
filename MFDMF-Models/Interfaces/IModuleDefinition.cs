namespace MFDMF_Models.Interfaces
{
	using MFDMF_Models.Models;
	using System.Collections.Generic;

	public interface IModuleDefinition : IImagePath, IReadableObject, IModuleName
	{
		List<ConfigurationDefinition> Configurations { get; set; }
		string DisplayName { get; set; }
		string Category { get; set; }
		string DCSName { get; set; }
	}
}