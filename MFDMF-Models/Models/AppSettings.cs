using System.Collections.Generic;

namespace MFDMF_Models.Models
{
	public class AppSettings
	{
		public string DisplayConfigurationFile { get; set; }
		public bool? ShowTooltips { get; set; }
		public bool? UseCougar { get; set; }
		public bool? SaveCroppedImages { get; set; }
		public string DefaultConfiguration { get; set; }
		public string FilePath { get; set; }
		public List<string> ModuleNames { get; set; }
		public bool? TurnOffCache { get; set; }
	}
}
