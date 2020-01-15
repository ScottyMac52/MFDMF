using MFDMF_Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MFDMF_Models.Models
{
    /// <summary>
    /// Defines a Configuration of an image displayed
    /// </summary>
    [JsonObject("configuration")]
    public class ConfigurationDefinition : ConfigurationBaseDefinition, IConfigurationDefinition, IReadableObject, IDisplayGeometry
    {
        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigurationDefinition() : base()
        {
            SubConfigurations = new List<SubConfigurationDefinition>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dc"></param>
        public ConfigurationDefinition(ConfigurationDefinition dc) : base(dc)
        {
            Left = dc.Left;
            Top = dc.Top;
            Width = dc.Width;
            Height = dc.Height;
            Opacity = dc.Opacity;
            SubConfigurations = new List<SubConfigurationDefinition>();
            SubConfigurations.AddRange(dc.SubConfigurations);
        }

        #endregion Ctor

        #region Basic Image Properties Left, Top, Width, Height and Opacity
        /// <summary>
        /// Width of the displayed image
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
        /// <summary>
        /// The Height of the displayed image
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
        /// <summary>
        /// Left coordinate of the displayed image
        /// </summary>
        [JsonProperty("left")]
        public int Left { get; set; }
        /// <summary>
        /// Top coordinate of the displayed image
        /// </summary>
        [JsonProperty("top")]
        public int Top { get; set; }

        #endregion Basic Image Properties Left, Top, Width, Height and Opacity

        #region SubConfiguration support

        /// <summary>
        /// List of SubConfigurations
        /// </summary>
        [JsonProperty("subConfigDef")]
        public virtual List<SubConfigurationDefinition> SubConfigurations { get; set; }

        #endregion SubConfiguration support

        #region Public overrides 

        /// <summary>
        /// Returns the portion of the readable string specific to <seealso cref="ConfigurationDefinition"/>
        /// </summary>
        /// <returns></returns>
        public override string ToReadableString()
        {
            return $"{Name} at ({Left}, {Top}) for ({Width}, {Height})";
        }

        public override string ToJson()
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

        protected override string GetReadableString()
        {
            return base.ToReadableString();
        }

        #endregion Public overrides 

    }
}