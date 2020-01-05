using MFDMF_Models.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace MFDMF_Models.Models
{
    [JsonObject("modules")]
    public class MFDMFConfiguration : IMFDMFDefinition, IReadableObject
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        [JsonConstructor()]
        public MFDMFConfiguration(ILoggerFactory loggerFactory)
        {
            Modules = new List<ModuleDefinition>();
            Displays = new List<DisplayDefinition>();
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger(typeof(MFDMFConfiguration));
        }

        public MFDMFConfiguration(ILoggerFactory loggerFactory, MFDMFConfiguration copy) : this(loggerFactory)
        {
            FilePath = copy?.FilePath;
            DefaultConfig = copy?.DefaultConfig;
            Modules.AddRange(copy?.Modules);
            Displays.AddRange(copy?.Displays);
        }

        public MFDMFConfiguration(ILoggerFactory loggerFactory, IMFDMFDefinition copy) : this(loggerFactory)
        {
            FilePath = copy?.FilePath;
            DefaultConfig = copy?.DefaultConfig;
            Modules.AddRange(copy?.Modules);
            Displays.AddRange(copy?.Displays);
        }

        #endregion Ctor

        #region Public static Create methods

        /// <summary>
        /// Converts JSON to a <seealso cref="MFDMFConfiguration"/>
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns><seealso cref="MFDMFConfiguration"/></returns>
        public static MFDMFConfiguration FromJson(ILoggerFactory loggerFactory, string jsonString)
        {
            var logger = loggerFactory?.CreateLogger<MFDMFConfiguration>();
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                };
                return JsonConvert.DeserializeObject<MFDMFConfiguration>(jsonString, settings);
            }
            catch (Exception ex)
            {
                logger?.LogError($"Unable to create MFDMFConfiguration, Exception: {ex}");
            }
            return null;
        }

        #endregion Public static Create methods

        #region MFDMF properties

        /// <summary>
        /// The path to the graphic files from the CTS utility
        /// </summary>
        [JsonProperty("filePath")]
        public string FilePath { get; set; }
        /// <summary>
        /// Default configuration to load on startup
        /// </summary>
        [JsonProperty("defaultConfig")]
        public string DefaultConfig { get; set; }
        /// <summary>
        /// List of modules available
        /// </summary>
        [JsonProperty("modules")]
        public List<ModuleDefinition> Modules { get; set; }

        [JsonProperty("displays")]
        public List<DisplayDefinition> Displays { get; set; }

        #endregion MFDMF properties

        #region Public overrides 

        /// <summary>
        /// Get the human readable string of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToReadableString();
        }

        /// <summary>
        /// Returns the portion of the readable string specific to <seealso cref="MFDMFConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public string ToReadableString()
        {
            return $"Modules Definition: FilePath: {FilePath} DefaultConfig: {DefaultConfig} Modules: {Modules?.Count ?? 0}";
        }

        /// <summary>
        /// Returns JSON for <seealso cref="MFDMFConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(this);
                return jsonString;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unable to convert type {GetType().Name} to Json. Exception {ex}");
            }
            return null;
        }

        #endregion Public overrides 
    }
}
