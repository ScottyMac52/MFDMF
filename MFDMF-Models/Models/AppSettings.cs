using MFDMF_Models.Models.TestPattern;
using System.Collections.Generic;

namespace MFDMF_Models.Models
{
	public class AppSettings
	{
		public string AppTitle { get; set; }
		public string LoggingThreshold { get; set; }
		public string DisplayConfigurationFile { get; set; }
		public bool? ShowTooltips { get; set; }
		public bool? SaveCroppedImages { get; set; }
		public string DefaultConfiguration { get; set; }
		public string FilePath { get; set; }

		/// <summary>
		/// List of Modules to load
		/// </summary>
		public List<string> ModuleNames { get; set; }
		/// <summary>
		/// List of images that are used for test patterns
		/// </summary>
		public List<ImageEntry> ImageList { get; set; }
		/// <summary>
		/// List of test patterns
		/// </summary>
		public List<TestPatternDefinition> PatternList { get; set; }
	}
}
