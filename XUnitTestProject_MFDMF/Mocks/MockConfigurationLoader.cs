using MFDMF_Models;
using MFDMF_Models.Interfaces;
using MFDMF_Services;
using System.Collections.Generic;
using System.IO;

namespace XUnitTestProject_MFDMF.Mocks
{
    public class MockConfigurationLoader : IConfigurationLoadingService
	{
        public string BasePath { get; set; }
        public string FileName { get; set; }
        public string BaseName { get; set; }
        public int ModuleCount { get; set; }
        public int ConfigurationCount { get; set; }
        public int SubConfigurationCount { get; set; }

        public IMFDMFDefinition LoadConfiguration()
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
        public IMFDMFDefinition GetTestData(string basePath, string fileName, string baseName, int moduleCount, int configurationCount, int subConfigurationCount)
        {
            var modulesDef = new MFDMFConfiguration()
            {
                DefaultConfig = $"{baseName}Module",
                FilePath = Path.Combine(basePath, fileName),
                Modules = new List<ModuleDefinition>()
            };

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
                        SubConfigurations = new List<SubConfigurationDefinition>()
                    };
                    for (int z = 1; z <= subConfigurationCount; z++)
                    {
                        var newSubConfiguration = new SubConfigurationDefinition()
                        {
                            Enabled = true,
                            StartX = 0,
                            EndX = 10,
                            StartY = 0,
                            EndY = 10,
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
                modulesDef.Modules.Add(newModule);
            };
            return modulesDef;
        }
    }
}
