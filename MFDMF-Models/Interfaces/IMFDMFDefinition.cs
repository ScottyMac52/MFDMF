using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Models.Interfaces
{
    public interface IMFDMFDefinition : IImagePath, IReadableObject
	{
        /// <summary>
        /// Default configuration to load on startup
        /// </summary>
        string DefaultConfig { get; set; }
        /// <summary>
        /// List of modules available
        /// </summary>
        List<ModuleDefinition> Modules { get; set; }
    }
}
