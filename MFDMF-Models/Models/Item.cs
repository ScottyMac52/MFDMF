namespace MFDMF_Models.Models
{
    using MFDMF_Models.Interfaces;
    using System.Collections.Generic;

    public class Item
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public IEnumerable<IModuleDefinition> ModuleDefinitions { get; set; } = new List<IModuleDefinition>();
    }
}
