using MFDMF_Models;
using MFDMF_Models.Interfaces;
using Microsoft.Extensions.Options;
using System.IO;

namespace MFDMF_Services
{
    public class ConfigurationLoadingService : IConfigurationLoadingService
    {
        private readonly AppSettings _settings;

        public ConfigurationLoadingService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public IMFDMFDefinition LoadConfiguration()
        {
            return LoadConfigturation(Path.Combine(Directory.GetCurrentDirectory(), _settings.ConfigurationFile));
        }

        /// <summary>
        /// Loads the Configuration from the specified path
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        private IMFDMFDefinition LoadConfigturation(string jsonFile)
        {
            var fileContent = File.ReadAllText(jsonFile);
            return MFDMFConfiguration.FromJson(fileContent);
        }

    }
}
