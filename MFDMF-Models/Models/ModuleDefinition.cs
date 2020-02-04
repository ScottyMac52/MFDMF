using MFDMF_Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFDMF_Models.Models
{
    [JsonObject("module")]
    public class ModuleDefinition : IModuleDefinition
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="copy"></param>
        [JsonConstructor()]
        public ModuleDefinition(ModuleDefinition copy = null) 
        {
            ModuleName ??= copy?.ModuleName;
            DisplayName ??= copy?.DisplayName;
            FileName ??= copy?.FileName;
            FilePath ??= copy?.FilePath;
            Enabled ??= copy?.Enabled;
            if((copy?.Configurations?.Count ?? 0) > 0)
            {
                Configurations.AddRange(copy?.Configurations);
            }
        }

        #endregion Ctor

        #region Module Definition properties

        /// <summary>
        /// Name of the Module
        /// </summary>
		[JsonProperty("name")]
        public string ModuleName { get; set; }
        /// <summary>
        /// Display Name for the Module
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        /// <summary>
        /// Filename to use for the entire module
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        /// <summary>
        /// Default path for the files
        /// </summary>
        [JsonProperty("filePath")]
        public string FilePath { get; set; }
        /// <summary>
        /// Can be used to disable a module and all it's configurations
        /// </summary>
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
        [JsonProperty("configurations")]
        /// <summary>
        /// The list of Configurations for this Module
        /// </summary>
        public List<ConfigurationDefinition> Configurations { get; set; }

		#endregion Module Definition properties 

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
        /// Returns the portion of the readable string specific to <seealso cref="ModuleDefinition"/>
        /// </summary>
        /// <returns></returns>
        public string ToReadableString()
        {
            return $"Module: {ModuleName} Display: {DisplayName} Filename: {FileName} Configurations: {Configurations.Where(con => (con.Enabled ?? false) == true).ToList().Count}";
        }

        /// <summary>
        /// Returns JSON for <seealso cref="ModuleDefinition"/>
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