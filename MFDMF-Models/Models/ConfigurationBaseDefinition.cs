using MFDMF_Models.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MFDMF_Models.Models
{
    /// <summary>
    /// Abstract definition of a Configuration item that is displayed as an image
    /// </summary>
    public abstract class ConfigurationBaseDefinition : IConfigurationDefinition
    {
        #region Constants

        private const int HASH_START = 17;
        private const int HASH_NUM = 23;

        #endregion Constants

        #region Utilities 

        /// <summary>
        /// Logger
        /// </summary>
        [JsonIgnore()]
        public ILogger Logger { get; internal set; }

        #endregion Utilities 

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigurationBaseDefinition()
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="cb"></param>
        public ConfigurationBaseDefinition(ConfigurationBaseDefinition cb)
        {
            Enabled = cb.Enabled;
            Logger = cb.Logger;
            FileName = cb.FileName;
            ModuleName = cb.ModuleName;
            Name = cb.Name;
            XOffsetStart = cb.XOffsetStart;
            XOffsetFinish = cb.XOffsetFinish;
            YOffsetStart = cb.YOffsetStart;
            YOffsetFinish = cb.YOffsetFinish;
            Opacity = cb.Opacity;
        }

        #endregion Ctor

        #region IImagePath properties

        /// <summary>
        /// FileName for the image cropping that this configuration uses
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// The starting file path for the configuration
        /// </summary>
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        /// <summary>
        /// Determines if the configuration is rendered
        /// </summary>
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }

        #endregion IImagePath properties

        #region IModuleName properties

        /// <summary>
        /// Module Name
        /// </summary>
        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        #endregion IModuleName properties

        #region Identifying properties 

        /// <summary>
        /// Name of the Configuration
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Default readable string
        /// </summary>
        /// <returns></returns>
        protected abstract string GetReadableString();

        #endregion Identifying properties

        #region Image cropping properties IOffsetGeometry

        /// <summary>
        /// Translucency of the image expressed as percentage of solidness 
        /// </summary>
        [JsonProperty("opacity")]
        public float Opacity { get; set; }
        /// <summary>
        /// Starting X position of the Crop
        /// </summary>
        [JsonProperty("xOffsetStart")]
        public int XOffsetStart { get; set; }
        /// <summary>
        /// Starting Y position of the Crop
        /// </summary>
        [JsonProperty("yOffsetStart")]
        public int YOffsetStart { get; set; }
        /// <summary>
        /// Ending X position of the Crop
        /// </summary>
        [JsonProperty("xOffsetFinish")]
        public int XOffsetFinish { get; set; }
        /// <summary>
        /// Ending Y position of the Crop
        /// </summary>
        [JsonProperty("yOffsetFinish")]
        public int YOffsetFinish { get; set; }

        #endregion Image cropping properties IOffsetGeometry

        #region Public overrides

        /// <summary>
		/// Determines Equality
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
        {
            var compareTo = obj as DisplayDefinition;
            return GetHashCode().Equals(compareTo.GetHashCode());
        }

        /// <summary>
        /// Hashcode implemented since Equals is implemented
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HASH_START;
                hashCode += (Name?.GetHashCode() ?? 0) * HASH_NUM;
                hashCode += (ModuleName?.GetHashCode() ?? 0) * HASH_NUM;
                hashCode += (FileName?.GetHashCode() ?? 0) * HASH_NUM;
                hashCode += (FilePath?.GetHashCode() ?? 0) * HASH_NUM;
                hashCode += Enabled.GetHashCode() * HASH_NUM;
                hashCode += Opacity.GetHashCode() * HASH_NUM;
                hashCode += XOffsetStart * HASH_NUM;
                hashCode += XOffsetFinish * HASH_NUM;
                hashCode += YOffsetStart * HASH_NUM;
                hashCode += YOffsetFinish * HASH_NUM;
                return hashCode;
            }
        }

        /// <summary>
        /// Describes the Configuration
        /// </summary>
        /// <returns></returns>
        public virtual string ToReadableString()
        {
            string completePath = null;
            string fileStatus = null;
            if (!string.IsNullOrEmpty(FilePath) && Directory.Exists(FilePath) && !string.IsNullOrEmpty(FileName))
            {
                completePath = Path.Combine(FilePath, FileName);
                var pathExists = File.Exists(completePath);
                fileStatus = pathExists ? "found " : "not found ";
            }

            return $"{Name} for: {ModuleName} at: {GetReadableString()} with Opacity: {Opacity} Enabled: {Enabled} from: {completePath ?? "Unknown Image"} was: {fileStatus} at Offset: ({XOffsetStart}, {YOffsetStart}) for: ({XOffsetFinish - XOffsetStart}, {YOffsetFinish - YOffsetStart}).";
        }

        /// <summary>
        /// Short form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToReadableString();
        }

        public virtual string ToJson()
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(this);
                return jsonString;
            }
            catch(Exception ex)
            {
                return $"{{ \"message\": \"{ex.Message}\", \"exceptionType\": \"{ex.GetType().Name}\"}}";
            }
        }

        #endregion Public overrides
    }
}
