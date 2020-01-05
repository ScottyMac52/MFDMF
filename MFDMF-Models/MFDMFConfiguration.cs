using MFDMF_Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace MFDMF_Models
{
    [JsonObject("modules")]
    public class MFDMFConfiguration : IMFDMFDefinition, IReadableObject
    {
        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        [JsonConstructor()]
        public MFDMFConfiguration()
        {
            Modules = new List<ModuleDefinition>();
            Displays = new List<DisplayDefinition>();
        }

        public MFDMFConfiguration(MFDMFConfiguration copy) : this()
        {
            FilePath = copy.FilePath;
            DefaultConfig = copy.DefaultConfig;
            Modules.AddRange(copy.Modules);
            Displays.AddRange(copy.Displays);
        }

        public MFDMFConfiguration(IMFDMFDefinition copy) : this()
        {
            FilePath = copy.FilePath;
            DefaultConfig = copy.DefaultConfig;
            Modules.AddRange(copy.Modules);
            Displays.AddRange(copy.Displays);
        }

        #endregion Ctor

        #region Public static Create methods

        /// <summary>
        /// Converts JSON to a <seealso cref="MFDMFConfiguration"/>
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns><seealso cref="MFDMFConfiguration"/></returns>
        public static MFDMFConfiguration FromJson(string jsonString)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };
            return JsonConvert.DeserializeObject<MFDMFConfiguration>(jsonString, settings);
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
                return $"{{ \"message\": \"{ex.Message}\", \"exceptionType\": \"{ex.GetType().Name}\"}}";
            }
        }

        #endregion Public overrides 
    }
}
