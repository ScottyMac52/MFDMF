namespace MFDMF_Models.Models
{
	using MFDMF_Models.Extensions;
	using MFDMF_Models.Interfaces;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Defines a Configuration of an image displayed
	/// </summary>
	[JsonObject("configuration")]
    public class ConfigurationDefinition : IConfigurationDefinition
    {
        #region Constants

        private const int HASH_START = 17;
        private const int HASH_NUM = 23;

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
        public ConfigurationDefinition(IConfigurationDefinition dc)
        {
            Parent = dc.Parent;
            Name = dc.Name;
            DisplayName = dc.DisplayName;
            FilePath = dc.FilePath;
            FileName = dc.FileName;
            ModuleName = dc.ModuleName;
            Enabled = dc?.Enabled ?? true;
            Left = dc.Left;
            Top = dc.Top;
            Width = dc.Width;
            Height = dc.Height;
            Center = dc.Center;
            Opacity = dc.Opacity;
            XOffsetStart = dc.XOffsetStart;
            XOffsetFinish = dc.XOffsetFinish;
            YOffsetStart = dc.YOffsetStart;
            YOffsetFinish = dc.YOffsetFinish;
            RulerName = dc.RulerName;
            ThrottleType = dc.ThrottleType;
            UseAsSwitch = dc.UseAsSwitch;
            SubConfigurations = new List<ConfigurationDefinition>();
            SubConfigurations.AddRange(dc.SubConfigurations);
        }

        #endregion Ctor

        #region Utilities 

        /// <summary>
        /// Logger
        /// </summary>
        [JsonIgnore()]
        public ILogger Logger { get; set; }

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

        /// <summary>
        /// Identifies the display that used as a basis for the configuration
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Throttle type specified in the configuration
        /// </summary>
        [JsonIgnore()]
        public string ThrottleType { get; set; }

        [JsonIgnore()]
        public string RulerName { get; set; }
        
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

		#endregion SubConfiguration support

		#region Cropping properties 

		/// <inheritdoc/>
		public Rectangle CroppingArea => GetCroppingArea();

        /// <inheritdoc/>
        public int CroppedWidth => GetCroppingArea().Width;

        /// <inheritdoc/>
        public int CroppedHeight => GetCroppingArea().Height;

        /// <inheritdoc/>
        public Point CroppingStart => GetCroppingArea().Location;

        #endregion Cropping properties 

        #region Validation

        public bool IsValid => CheckConfiguration();

		#endregion Validation

		#region Public methods

		/// <summary>
		/// Gets the center coordinates for the current Configuration inside the specified Configuration
		/// </summary>
		/// <param name="configurationDefinition"></param>
		public Point GetCenterTo(IDisplayGeometry displayGeometry)
        {
            var size = new Size(Width ?? 0, Height ?? 0);
            var centerPoint = size.RelativeCenterInRectangle(new Size(displayGeometry?.Width ?? 0, displayGeometry?.Height  ?? 0));
            return new Point(centerPoint.X, centerPoint.Y);
        }

        /// <summary>
        /// Checks to see if the selected Configuration has been selected via their configuration or the command line options
        /// </summary>
        /// <param name="specifiedSubConfigs"></param>
        /// <returns></returns>
        public bool CheckForActiveSelectedSubConfiguration(List<string> specifiedSubConfigs)
        {
            var useAsStatic = ((UseAsSwitch ?? false) == false) && (Enabled ?? false);
            var useAsSwitch = (UseAsSwitch ?? false);
            var selected = specifiedSubConfigs?.Any(ss => Name?.Equals(ss, StringComparison.InvariantCultureIgnoreCase) == true) ?? false;
            return useAsStatic || (useAsSwitch && selected);
        }

        /// <summary>
        /// Gets the image prefix for the configuration
        /// </summary>
        /// <param name="selectedSubMods"></param>
        /// <returns></returns>
        public string GetImagePrefix(List<string> selectedSubMods)
        {
            var selectSubStrings = new List<string>();
            var hashCode = GetHashCode();
            ConfigurationDefinition.WalkConfigurationDefinitionsWithAction(this, (subConfig) =>
            {
                if(subConfig.CheckForActiveSelectedSubConfiguration(selectedSubMods))
                {
                    hashCode += subConfig.GetHashCode();
                    if (selectedSubMods.Contains(subConfig.Name))
                    {
                        selectSubStrings.Add(subConfig.Name);
                    }
                }
            });

            if(selectSubStrings.Count > 0)
			{
                return $"{ModuleName}-{Name}-{string.Join('-',selectSubStrings)}-{hashCode}";
            }
            else
			{
                return $"{ModuleName}-{Name}{hashCode}";
            }

        }

        /// <summary>
        /// Recursive processing of the <see cref="IConfigurationDefinition"/> for a Module
        /// </summary>
        /// <param name="configurationDefinition"></param>
        public static void WalkConfigurationDefinitionsWithAction(IConfigurationDefinition configurationDefinition, Action<IConfigurationDefinition> actionToInvoke)
        {
            configurationDefinition?.SubConfigurations?.ForEach(subConfig =>
            {
                actionToInvoke?.Invoke(subConfig);
                WalkConfigurationDefinitionsWithAction(subConfig, actionToInvoke);
            });
        }

		#endregion Public methods

		#region Public overrides 

		public override string ToString()
		{
            return ToReadableString();
        }

		public override bool Equals(object obj)
        {
            var config2 = obj as ConfigurationDefinition;
            return GetHashCode().Equals(config2?.GetHashCode() ?? 0);
        }

        public override int GetHashCode()
        {
            var hashCode = HASH_START;
            hashCode += (ModuleName?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (Name?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (FileName?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (FilePath?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (Height?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Width?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Left?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Top?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Center?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Enabled?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (Opacity?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (RulerName?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (ThrottleType?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
            hashCode += (UseAsSwitch?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (XOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (XOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (YOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
            hashCode += (YOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
            return hashCode;
        }

        /// <summary>
        /// Returns the portion of the readable string specific to <seealso cref="ConfigurationDefinition"/>
        /// </summary>
        /// <returns></returns>
        public virtual string ToReadableString()
        {
            string fileName;
            if(!string.IsNullOrEmpty(FileName))
            {
                var fi = new FileInfo(FileName);
                fileName = fi.Name.Replace(fi.Extension, "");
            }
            else
            {
                fileName = "None";
            }
            var position = $"{Left ?? 0}-{Top ?? 0}-{Width ?? 0}-{Height ?? 0}";
            var offsetCoord = $"{(XOffsetStart ?? 0)}-{(YOffsetStart ?? 0)}";
            var offSetSize = $"{(XOffsetFinish ?? 0)-(XOffsetStart ?? 0)}-{(YOffsetFinish ?? 0)-(YOffsetStart ?? 0)}";
            var imagePrefix = $"{Parent?.Name ?? ""}-{fileName}-{Name}-{offsetCoord}-{offSetSize}-{position}-{Opacity ?? 1.0F}-{Center ?? false}-{ThrottleType}-{RulerName}";
            return imagePrefix;
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

        #region Private helpers
        private bool CheckConfiguration()
        {
            bool isValid = true;

            if ((Width ?? 0) < 0)
            {
                isValid = false;
            }

            if ((Height ?? 0) < 0)
            {
                isValid = false;
            }

            return isValid;
        }

        private Rectangle GetCroppingArea()
        {
            var point = new Point(XOffsetStart ?? 0, YOffsetStart ?? 0);
            var size = new Size((XOffsetFinish ?? 0) - (XOffsetStart ?? 0), (YOffsetFinish ?? 0) - (YOffsetStart ?? 0));
            return new Rectangle(point, size);
        }
        #endregion Private helpers
    }
}