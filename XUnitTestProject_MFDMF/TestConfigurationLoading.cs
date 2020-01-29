using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Services.Configuration;
using System.Collections.Generic;
using System.IO;
using Xunit;
using XUnitTestProject_MFDMF.Mocks;

namespace XUnitTestProject_MFDMF
{
    public class TestConfigurationLoading : BaseTesting
    {
        [Fact]
		public void Test_Model_ToJson_FromJson_NoSubConfigurations()
        {
            // ARRANNGE
            var baseFileName = "LMFD";
            var baseName = "Test";
            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"TestData\TestModule");
            IConfigurationLoadingService loadingService = new MockConfigurationLoader()
            {
                BaseName = baseName,
                BasePath = testDataPath,
                FileName = baseFileName,
                ModuleCount = 20,
                ConfigurationCount = 20,
                SubConfigurationCount = 0
            };

            // ACT
            var testData = loadingService.LoadModulesConfigurationFile(testDataPath, null);

            // ASSERT
            Validate(baseFileName, baseName, testDataPath, testData, 20, 20, 0);
        }

        /// <summary>
        /// Validates any model
        /// </summary>
        /// <param name="baseFileName"></param>
        /// <param name="baseName"></param>
        /// <param name="testDataPath"></param>
        /// <param name="testModulesDef"></param>
        /// <param name="moduleCount"></param>
        /// <param name="configCount"></param>
        /// <param name="subConfigCount"></param>
        private void Validate(string baseFileName, string baseName, string testDataPath, List<IModuleDefinition> testModulesDef, int moduleCount, int configCount, int subConfigCount)
        {
            var moduleCounter = 0;
            var configurationCounter = 0;
            var subConfigurationCounter = 0;

            testModulesDef?.ForEach(module =>
            {
                moduleCounter++;
                var moduleName = $"{baseName}Module{moduleCounter}";
                Assert.Equal($"{baseName} Module {moduleCounter}", module.DisplayName);
                Assert.Equal(moduleName, module.ModuleName);
                Assert.Equal($"{baseFileName}{moduleCounter}.png", module.FileName);

                module?.Configurations?.ForEach(configuration =>
                {
                    configurationCounter++;
                    var configurationName = $"{baseName}Config{configurationCounter}";
                    var configurationFilename = $"{baseFileName}{configurationCounter}.jpg";
                    Assert.Equal(moduleName, configuration.ModuleName);
                    Assert.Equal(configurationName, configuration.Name);
                    Assert.Equal(configurationFilename, configuration.FileName);
                    Assert.Equal(testDataPath, configuration.FilePath);
                    Assert.Equal(1, configuration.XOffsetStart);
                    Assert.Equal(50, configuration.XOffsetFinish);
                    Assert.Equal(1, configuration.YOffsetStart);
                    Assert.Equal(50, configuration.YOffsetFinish);
                    Assert.Equal(configurationCounter, configuration.Left);
                    Assert.Equal(configurationCounter, configuration.Top);
                    Assert.Equal(100 + (10 * configurationCounter), configuration.Height);
                    Assert.Equal(100 + (10 * configurationCounter), configuration.Width);

                    configuration?.SubConfigurations?.ForEach(subConfiguration =>
                    {
                        subConfigurationCounter++;
                        Assert.Equal($"{baseName}Module{moduleCounter}", subConfiguration.ModuleName);
                        Assert.Equal(testDataPath, subConfiguration.FilePath);
                        Assert.Equal($"{baseName}Inset{subConfigurationCounter}", subConfiguration.Name);
                        Assert.Equal($"inset{subConfigurationCounter}.jpg", subConfiguration.FileName);
                        Assert.Equal(0, subConfiguration.Left);
                        Assert.Equal(10, subConfiguration.Width);
                        Assert.Equal(0, subConfiguration.Top);
                        Assert.Equal(10, subConfiguration.Height);
                        Assert.Equal(1, subConfiguration.XOffsetStart);
                        Assert.Equal(50, subConfiguration.XOffsetFinish);
                        Assert.Equal(1, subConfiguration.YOffsetStart);
                        Assert.Equal(50, subConfiguration.YOffsetFinish);
                    });
                    Assert.Equal(subConfigCount, subConfigurationCounter);
                    subConfigurationCounter = 0;
                });
                Assert.Equal(configCount, configurationCounter);
                configurationCounter = 0;
            });
            Assert.Equal(moduleCount, moduleCounter);
            moduleCounter = 0;
        }
    }
}
