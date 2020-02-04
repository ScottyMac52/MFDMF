using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Services.Configuration
{
	public interface IConfigurationLoadingService
	{
		List<IModuleDefinition> LoadModulesConfigurationFile(string jsonFile, List<IDisplayDefinition> displays);
	}
}