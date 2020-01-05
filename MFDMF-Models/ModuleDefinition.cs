using MFDMF_Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MFDMF_Models
{
    [JsonObject("module")]
    public class ModuleDefinition : IReadableObject, IModuleDefinition
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>      
        [JsonConstructor()]
        public ModuleDefinition(MFDMFConfiguration parent = null)
        {
            Parent = parent;
            Configurations = new List<ConfigurationDefinition>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public ModuleDefinition(ModuleDefinition copy) : this(copy.Parent)
        {
            ModuleName = copy.ModuleName;
            DisplayName = copy.DisplayName;
            FileName = copy.FileName;
            Configurations.AddRange(copy.Configurations);
        }

        #endregion Ctor

        #region Module Definition properties Parent, ModuleName, DisplayName, FileName, Configurations

        /// <summary>
        /// Parent to this Module
        /// </summary>
        [JsonIgnore()]
        public MFDMFConfiguration Parent { get; internal set; }
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
        /// The list of Configurations for this Module
        /// </summary>
        [JsonProperty("configurations")]
        public List<ConfigurationDefinition> Configurations { get; set; }

		#endregion Module Definition properties Parent, ModuleName, DisplayName, FileName, Configurations

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
            return $"Parent: {Parent?.ToReadableString()} Module: {ModuleName} Display: {DisplayName} Filename: {FileName} Configurations: {Configurations?.Count ?? 0}";
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