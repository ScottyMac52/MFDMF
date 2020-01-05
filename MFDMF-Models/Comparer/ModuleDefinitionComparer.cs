﻿using System.Collections.Generic;

namespace MFDMF_Models.Comparer
{
    /// <summary>
    /// Comparer for Module Definitions
    /// </summary>
    public class ModuleDefinitionComparer : IComparer<ModuleDefinition>
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(ModuleDefinition x, ModuleDefinition y)
        {
            return x.DisplayName.CompareTo(y.DisplayName);
        }
    }
}
