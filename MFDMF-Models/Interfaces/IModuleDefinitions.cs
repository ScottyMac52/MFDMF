namespace MFDMF_Models.Interfaces
{
	using MFDMF_Models.Models;
	using System.Collections.Generic;

	public interface IModuleDefinitions
	{
		List<ModuleDefinition> Modules { get; set; }
	}
}
