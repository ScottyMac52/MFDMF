using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
    public interface IMFDMFDefinition
	{
        /// <summary>
        /// The path to the graphic files from the CTS utility
        /// </summary>
        string FilePath { get; set; }
        /// <summary>
        /// Default configuration to load on startup
        /// </summary>
        string DefaultConfig { get; set; }
        /// <summary>
        /// List of modules available
        /// </summary>
        List<ModuleDefinition> Modules { get; set; }
        /// <summary>
        /// List of Displays
        /// </summary>
        List<DisplayDefinition> Displays { get; set; }
    }
}
