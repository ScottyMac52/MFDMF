using MFDMF_Models.Extensions;
using MFDMF_Models.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MFDMF_Models.Models
{
    /// <summary>
    /// Defines a Configuration of an image displayed
    /// </summary>
    [JsonObject("configuration")]
    public class ConfigurationDefinition : IConfigurationDefinition
    {
        #region Constants

        private const int HASH_START = 17;
        private const int HASH_NUM = 19;

        #endregion Constants

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigurationDefinition()
        {
            SubConfigurations = new List<ConfigurationDefinition>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dc"></param>
        public ConfigurationDefinition(ConfigurationDefinition dc)
        {
            Name = dc.Name;
            FilePath = dc.FilePath;
            FileName = dc.FileName;
            ModuleName = dc.ModuleName;
            Enabled = dc?.Enabled ?? true;
            Left = dc.Left;
            Top = dc.Top;
            Width = dc.Width;
            Height = dc.Height;
            Opacity = dc.Opacity;
            XOffsetStart = dc.XOffsetStart;
            XOffsetFinish = dc.XOffsetFinish;
            YOffsetStart = dc.YOffsetStart;
            YOffsetFinish = dc.YOffsetFinish;
            SubConfigurations = new List<ConfigurationDefinition>();
            SubConfigurations.AddRange(dc.SubConfigurations);
        }

        #endregion Ctor

        #region Utilities 

        /// <summary>
        /// Logger
        /// </summary>
        [JsonIgnore()]
        public ILogger Logger { get; internal set; }

        #endregion Utilities 

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
        /// The parent of this configuration
        /// </summary>
        [JsonIgnore()]
        public IConfigurationDefinition Parent { get; set; }

        /// <summary>
        /// Name of the Configuration
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        #endregion Identifying properties

        #region Image cropping properties IOffsetGeometry

        /// <summary>
        /// If true and in a subconfiguration used as an inset switch
        /// </summary>
        [JsonProperty("useAsSwitch")]
        public bool? UseAsSwitch { get; set; }
        /// <summary>
        /// Translucency of the image expressed as percentage of solidness 
        /// </summary>
        [JsonProperty("opacity")]
        public float? Opacity { get; set; }
        /// <summary>
        /// Starting X position of the Crop
        /// </summary>
        [JsonProperty("xOffsetStart")]
        public int? XOffsetStart { get; set; }
        /// <summary>
        /// Starting Y position of the Crop
        /// </summary>
        [JsonProperty("yOffsetStart")]
        public int? YOffsetStart { get; set; }
        /// <summary>
        /// Ending X position of the Crop
        /// </summary>
        [JsonProperty("xOffsetFinish")]
        public int? XOffsetFinish { get; set; }
        /// <summary>
        /// Ending Y position of the Crop
        /// </summary>
        [JsonProperty("yOffsetFinish")]
        public int? YOffsetFinish { get; set; }

        #endregion Image cropping properties IOffsetGeometry

        #region Basic Image Properties Center, Left, Top, Width, Height and Opacity

        /// <summary>
        /// If true then the coordinates are set to center the configuration to it's Parent
        /// </summary>
        [JsonProperty("center")]
        public bool? Center { get; set; }
        /// <summary>
        /// Width of the displayed image
        /// </summary>
        [JsonProperty("width")]
        public int? Width { get; set; }
        /// <summary>
        /// The Height of the displayed image
        /// </summary>
        [JsonProperty("height")]
        public int? Height { get; set; }
        /// <summary>
        /// Left coordinate of the displayed image
        /// </summary>
        [JsonProperty("left")]
        public int? Left { get; set; }
        /// <summary>
        /// Top coordinate of the displayed image
        /// </summary>
        [JsonProperty("top")]
        public int? Top { get; set; }

        #endregion Basic Image Properties Center, Left, Top, Width, Height and Opacity

        #region SubConfiguration support

        /// <summary>
        /// List of SubConfigurations
        /// </summary>
        [JsonProperty("subConfigDef")]
        public virtual List<ConfigurationDefinition> SubConfigurations { get; set; }

        /// <summary>
        /// If true then the superimposed bitmap is opaque
        /// </summary>
        [JsonProperty("makeOpaque")]
        public bool? MakeOpaque { get; set; }

		#endregion SubConfiguration support

		#region Public methods

        /// <summary>
        /// Centers the current Configuration to it's Parent
        /// </summary>
        public void SetCenterToParent()
        {
            var point = GetCenterTo(Parent);
            Left = point.X;
            Top = point.Y;
        }

        /// <summary>
        /// Centers the current Configuration inside the specified Configuration
        /// </summary>
        /// <param name="configurationDefinition"></param>
        public Point GetCenterTo(IDisplayGeometry displayGeometry)
        {
            var size = new Size(Width ?? 0, Height ?? 0);
            var centerPoint = size.RelativeCenterInRectangle(new Size(displayGeometry?.Width ?? 0, displayGeometry?.Height  ?? 0));
            return new Point(centerPoint.X, centerPoint.Y);
        }

        /// <summary>
        /// Recursive processing of the <see cref="IConfigurationDefinition"/> for a Module
        /// </summary>
        /// <param name="configurationDefinition"></param>
        public static void WalkConfigurattionDefinitionsWithAction(IConfigurationDefinition configurationDefinition, Action<IConfigurationDefinition> actionToInvoke)
        {
            configurationDefinition?.SubConfigurations?.ForEach(subConfig =>
            {
                actionToInvoke?.Invoke(subConfig);
                WalkConfigurattionDefinitionsWithAction(subConfig, actionToInvoke);
            });
        }

        #endregion Public methods

        #region Public overrides 

        public override bool Equals(object obj)
        {
            var config2 = obj as ConfigurationDefinition;
            return GetHashCode().Equals(config2?.GetHashCode() ?? 0);
        }

        public override int GetHashCode()
        {
            var hashCode = HASH_START;
            hashCode += Name.GetHashCode() * HASH_NUM;
            hashCode += FilePath.GetHashCode() * HASH_NUM;
            hashCode += FileName.GetHashCode() * HASH_NUM;
            hashCode += ModuleName.GetHashCode() * HASH_NUM;
            hashCode += Height.GetHashCode() * HASH_NUM;
            hashCode += Left.GetHashCode() * HASH_NUM;
            hashCode += Top.GetHashCode() * HASH_NUM;
            hashCode += (Enabled?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Opacity?.GetHashCode() ?? 0 ) * HASH_NUM;
            hashCode += (XOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (XOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (YOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (YOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
            SubConfigurations?.ForEach(subConfig =>
            {
                hashCode += subConfig.GetHashCode();
            });
            return hashCode;
        }

        /// <summary>
        /// Returns the portion of the readable string specific to <seealso cref="ConfigurationDefinition"/>
        /// </summary>
        /// <returns></returns>
        public virtual string ToReadableString()
        {
            Point point;

            var fileName = string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(FilePath) ? "None" : Path.Combine(FilePath,FileName);
            point = new Point(Left ?? 0, Top ?? 0);
            if(Center ?? false)
            {
                point = GetCenterTo(Parent);
            }
           return $"ImageSource: {fileName} Cropped from: ({XOffsetStart ?? 0},{YOffsetStart ?? 0}) to ({(XOffsetFinish ?? 0) - (XOffsetStart ?? 0)}, {(YOffsetFinish ?? 0) - (YOffsetStart ?? 0)}) for {Name} at ({point.X}, {point.Y}) for ({Width}, {Height}) ";
        }

        public virtual string ToJson()
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