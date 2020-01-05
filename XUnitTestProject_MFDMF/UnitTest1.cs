using MFDMF_Models;
using MFDMF_Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using XUnitTestProject_MFDMF.Mocks;

namespace XUnitTestProject_MFDMF
{
	public class UnitTest1
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
                SubConfigurationCount = 20
            };
            var testData = loadingService.LoadConfiguration();

            // ACT
            var testJsonObject = new MFDMFConfiguration(testData);
            var serializedModules = testJsonObject.ToJson();
            var testModulesDef = MFDMFConfiguration.FromJson(serializedModules);

            // ASSERT
            Validate(baseFileName, baseName, testDataPath, serializedModules, testModulesDef);
        }

        [Fact]
        public void Test_Model_From_File_Matches()
        {
            // ARRANGE
            var baseFileName = "LMFD";
            var baseName = "Test";
            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"TestData\TestModule");
            IConfigurationLoadingService loadingService = new MockConfigurationLoader()
            {
                BaseName = baseName,
                BasePath = testDataPath,
                FileName = baseFileName,
                ModuleCount = 20,
                ConfigurationCount = 5,
                SubConfigurationCount = 5
            };
            var testData = loadingService.LoadConfiguration();

            // ACT
            var fileData = GetFileData(Path.Combine(testDataPath, "testData.json")).GetAwaiter().GetResult();

            // ASSERT
            Assert.Equal(testData.DefaultConfig, fileData.DefaultConfig);
        }

        private async Task<MFDMFConfiguration> GetFileData(string jsonFile)
        {
            var fileContent = await File.ReadAllTextAsync(jsonFile);
            return MFDMFConfiguration.FromJson(fileContent);
        }

        /// <summary>
        /// Validates any model
        /// </summary>
        /// <param name="baseFileName"></param>
        /// <param name="baseName"></param>
        /// <param name="testDataPath"></param>
        /// <param name="serializedModules"></param>
        /// <param name="testModulesDef"></param>
        private void Validate(string baseFileName, string baseName, string testDataPath, string serializedModules, MFDMFConfiguration testModulesDef)
        {
            var moduleCounter = 1;
            var configurationCounter = 1;
            var subConfigurationCounter = 1;

            Assert.Contains($"{baseName}Module", serializedModules);
            Assert.Equal($"{baseName}Module", testModulesDef.DefaultConfig);

            testModulesDef?.Modules?.ForEach(module =>
            {
                var moduleName = $"{baseName}Module{moduleCounter}";
                Assert.Equal($"{baseName} Module {moduleCounter}", module.DisplayName);
                Assert.Equal(moduleName, module.ModuleName);
                Assert.Equal($"{baseFileName}{moduleCounter}.png", module.FileName);

                module?.Configurations?.ForEach(configuration =>
                {
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
                        Assert.Equal($"{baseName}Module{moduleCounter}", subConfiguration.ModuleName);
                        Assert.Equal(testDataPath, subConfiguration.FilePath);
                        Assert.Equal($"{baseName}Inset{subConfigurationCounter}", subConfiguration.Name);
                        Assert.Equal($"inset{subConfigurationCounter}.jpg", subConfiguration.FileName);
                        Assert.Equal(0, subConfiguration.StartX);
                        Assert.Equal(10, subConfiguration.EndX);
                        Assert.Equal(0, subConfiguration.StartY);
                        Assert.Equal(10, subConfiguration.EndY);
                        Assert.Equal(1, subConfiguration.XOffsetStart);
                        Assert.Equal(50, subConfiguration.XOffsetFinish);
                        Assert.Equal(1, subConfiguration.YOffsetStart);
                        Assert.Equal(50, subConfiguration.YOffsetFinish);
                        subConfigurationCounter++;
                    });
                    subConfigurationCounter = 1;
                    configurationCounter++;
                });
                configurationCounter = 1;
                moduleCounter++;
            });
        }
    }
}
