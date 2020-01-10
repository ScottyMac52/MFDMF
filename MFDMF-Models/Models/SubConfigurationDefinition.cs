using MFDMF_Models.Interfaces;
using Newtonsoft.Json;
using System;

namespace MFDMF_Models.Models
{
    [JsonObject("sub-configuration")]
    public class SubConfigurationDefinition : ConfigurationBaseDefinition, IConfigurationDefinition, IInsetDisplayGeometry
    {
        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SubConfigurationDefinition()
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dc"></param>
        public SubConfigurationDefinition(SubConfigurationDefinition dc) : base(dc)
        {
            StartX = dc.StartX;
            EndX = dc.EndX;
            StartY = dc.StartY;
            EndY = dc.EndY;
        }

        #endregion Ctor

        #region Basic Image Properties StartX, EndX, StartY and EndY - IInsetDisplayGeometry

        /// <summary>
        /// The X coordinate inside of the Configuration image relative to (0,0) to start rendering the image
        /// </summary>
        [JsonProperty("startX")]
        public int StartX { get; set; }
        /// <summary>
        /// The X coordinate inside of the Configuration image relative to (0,0) to end rendering the image
        /// </summary>
        [JsonProperty("endX")]
        public int EndX { get; set; }
        /// <summary>
        /// The Y coordinate inside of the Configuration image relative to (0,0) to start rendering the image
        /// </summary>
        [JsonProperty("startY")]
        public int StartY { get; set; }
        /// <summary>
        /// The Y coordinate inside of the Configuration image relative to (0,0) to end rendering the image
        /// </summary>
        [JsonProperty("endY")]
        public int EndY { get; set; }

        #endregion Basic Image Properties StartX, EndX, StartY and EndY - IInsetDisplayGeometry

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
        /// Returns the portion of the readable string specific to <seealso cref="SubConfigurationDefinition"/>
        /// </summary>
        /// <returns></returns>
        public override string ToReadableString()
        {
            return $"{GetReadableString()}_{EndX - StartX}_{EndY - StartY}";
        }

        /// <summary>
        /// Returns JSON for <seealso cref="SubConfigurationDefinition"/>
        /// </summary>
        /// <returns></returns>
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