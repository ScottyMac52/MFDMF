namespace MFDMF_Services.Configuration
{
	using MFDMF_Models.Interfaces;
	using MFDMF_Models.Models;
	using System.Collections.Generic;

	public interface IConfigurationLoadingService
	{
		IEnumerable<IModuleDefinition> LoadModulesConfigurationFile(string jsonContent, string category);
	}
}