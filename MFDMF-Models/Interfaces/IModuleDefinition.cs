using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
	public interface IModuleDefinition
	{
		List<ConfigurationDefinition> Configurations { get; set; }
		string DisplayName { get; set; }
		string FileName { get; set; }
		string ModuleName { get; set; }
		MFDMFConfiguration Parent { get; }
	}
}