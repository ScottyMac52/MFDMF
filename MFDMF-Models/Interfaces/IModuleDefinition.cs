using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
	public interface IModuleDefinition : IImagePath, IReadableObject, IModuleName
	{
		List<ConfigurationDefinition> Configurations { get; set; }
		string DisplayName { get; set; }
		IMFDMFDefinition Parent { get; }
		void PreProcessModule(IMFDMFDefinition parent);
	}
}