using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Services.Configuration
{
	public interface IConfigurationLoadingService
	{
		List<ModuleDefinition> LoadModulesConfigurationFile(string jsonFile);
	}
}