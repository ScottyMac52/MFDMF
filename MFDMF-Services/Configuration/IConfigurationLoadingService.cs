using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Services.Configuration
{
	public interface IConfigurationLoadingService
	{
		IMFDMFDefinition LoadConfiguration();

		List<ModuleDefinition> LoadModulesConfigurationFile(string jsonFile);

		ModuleDefinition LoadModuleConfigurationFile(string jsonFile);
	}
}