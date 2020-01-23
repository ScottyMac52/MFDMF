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
        private readonly ModComparisonType _modComparisonType;

        public ModuleDefinitionComparer(ModComparisonType modComparisonType = ModComparisonType.Name)
        {
            _modComparisonType = modComparisonType;
        }

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IModuleDefinition x, IModuleDefinition y)
        {
            if(_modComparisonType == ModComparisonType.DisplayName)
            {
                return x.DisplayName?.CompareTo(y?.DisplayName) ?? 0;
            }
            else
            {
                return x.ModuleName?.CompareTo(y?.ModuleName) ?? 0;
            }
        }
    }
}
