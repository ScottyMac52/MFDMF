namespace MFDMF_Services.Configuration
{
	using MFDMF_Models.Interfaces;
	using System.Collections.Generic;

	public interface IConfigurationLoadingService
	{
		IEnumerable<IModuleDefinition> LoadModulesConfigurationFile(string jsonContent, IEnumerable<IDisplayDefinition> displays, string category);
	}
}