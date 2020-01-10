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
        [JsonConstructor()]
        public ModuleDefinition(IMFDMFDefinition parent = null)
        {
            Parent = parent;
            Configurations = new List<ConfigurationDefinition>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public ModuleDefinition(IModuleDefinition copy) : this(copy.Parent)
        {
            ModuleName = copy.ModuleName;
            DisplayName = copy.DisplayName;
            FileName = copy.FileName;
            FilePath = copy.FilePath;
            Enabled = copy.Enabled;
            Configurations.AddRange(copy.Configurations);
        }

        #endregion Ctor

        #region Module Definition properties Parent, ModuleName, DisplayName, FileName, Configurations

        /// <summary>
        /// Parent to this Module
        /// </summary>
        [JsonIgnore()]
        public IMFDMFDefinition Parent { get; internal set; }
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

		#endregion Module Definition properties Parent, ModuleName, DisplayName, FileName, Configurations

		#region Public methods

		/// <summary>
		/// Ensures that the Hierarchy receives default values for important properties but ensures children can override
		/// </summary>
		/// <param name="parent"></param>
		public void PreProcessModule(IMFDMFDefinition parent)
        {
            Parent = parent;
            FilePath ??= parent.FilePath;
            FileName ??= parent.FileName;
            Enabled ??= parent.Enabled;
            
            Configurations?.ForEach(config =>
            {
                config.FilePath ??= FilePath;
                config.FileName ??= FileName;
                config.ModuleName ??= ModuleName;
                config.Enabled ??= Enabled;

                config?.SubConfigurations?.ForEach(subConfig =>
                {
                    subConfig.FilePath ??= config.FilePath;
                    subConfig.FileName ??= config.FileName;
                    subConfig.ModuleName ??= config.ModuleName;
                    subConfig.Enabled ??= config.Enabled;
                });
            });
        }

        #endregion Public methods

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
            return $"Parent: {Parent?.ToReadableString()} Module: {ModuleName} Display: {DisplayName} Filename: {FileName} Configurations: {Configurations.Where(con => (con.Enabled ?? false) == true).ToList().Count}";
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