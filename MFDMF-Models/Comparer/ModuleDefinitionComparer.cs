using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using System.Collections.Generic;

namespace MFDMF_Models.Comparer
{
    /// <summary>
    /// Comparer for Module Definitions
    /// </summary>
    public class ModuleDefinitionComparer : IComparer<IModuleDefinition>
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IModuleDefinition x, IModuleDefinition y)
        {
            return x.DisplayName.CompareTo(y.DisplayName);
        }
    }
}
