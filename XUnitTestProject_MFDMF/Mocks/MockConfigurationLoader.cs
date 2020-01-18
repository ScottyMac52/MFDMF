using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Services.Configuration;
using System.Collections.Generic;
using System.IO;

namespace XUnitTestProject_MFDMF.Mocks
{
    /// <summary>
    /// Interface mocked Configuration Loader
    /// </summary>
    public class MockConfigurationLoader : BaseTesting, IConfigurationLoadingService
	{
        public string BasePath { get; set; }
        public string FileName { get; set; }
        public string BaseName { get; set; }
        public int ModuleCount { get; set; }
        public int ConfigurationCount { get; set; }
        public int SubConfigurationCount { get; set; }

        public List<ModuleDefinition> LoadModulesConfigurationFile(string jsonFile)
        {
            return GetTestData(BasePath, FileName, BaseName, ModuleCount, ConfigurationCount, SubConfigurationCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="fileName"></param>
        /// <param name="baseName"></param>
        /// <param name="moduleCount"></param>
        /// <param name="configurationCount"></param>
        /// <param name="subConfigurationCount"></param>
        /// <returns></returns>
        public List<ModuleDefinition> GetTestData(string basePath, string fileName, string baseName, int moduleCount, int configurationCount, int subConfigurationCount)
        {
            var moduleDefintions = new List<ModuleDefinition>();

            for (int x = 1; x <= moduleCount; x++)
            {
                var newModule = new ModuleDefinition()
                {
                    DisplayName = $"{baseName} Module {x}",
                    ModuleName = $"{baseName}Module{x}",
                    FileName = $"{fileName}{x}.png",
                    Configurations = new List<ConfigurationDefinition>()
                };
                for (int y = 1; y <= configurationCount; y++)
                {
                    var newConfig = new ConfigurationDefinition()
                    {
                        Name = $"{baseName}Config{y}",
                        ModuleName = $"{baseName}Module{x}",
                        FileName = $"{fileName}{y}.jpg",
                        Enabled = true,
                        FilePath = basePath,
                        Height = 100 + 10 * y,
                        Width = 100 + 10 * y,
                        Left = y,
                        Top = y,
                        XOffsetStart = 1,
                        XOffsetFinish = 50,
                        YOffsetStart = 1,
                        YOffsetFinish = 50,
                        SubConfigurations = new List<ConfigurationDefinition>()
                    };
                    for (int z = 1; z <= subConfigurationCount; z++)
                    {
                        var newSubConfiguration = new ConfigurationDefinition()
                        {
                            Enabled = true,
                            Left = 0,
                            Width = 10,
                            Top = 0,
                            Height = 10,
                            FileName = $"inset{z}.jpg",
                            FilePath = basePath,
                            ModuleName = newModule.ModuleName,
                            Name = $"{baseName}Inset{z}",
                            XOffsetStart = 1,
                            XOffsetFinish = 50,
                            YOffsetStart = 1,
                            YOffsetFinish = 50
                        };
                        newConfig.SubConfigurations.Add(newSubConfiguration);
                    }
                    newModule.Configurations.Add(newConfig);
                }
                moduleDefintions.Add(newModule);
            };

            return moduleDefintions;
        }
    }
}
