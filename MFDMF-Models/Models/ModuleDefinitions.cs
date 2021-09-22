namespace MFDMF_Models.Models
{
	using MFDMF_Models.Interfaces;
	using Newtonsoft.Json;
	using System.Collections.Generic;

	public class ModuleDefinitions : IModuleDefinitions
	{
		[JsonProperty("modules")]
		public List<ModuleDefinition> Modules { get; set; }
	}
}
