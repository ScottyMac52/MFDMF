using MFDMF_Models.Extensions;
using MFDMF_Models.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace MFDMF_Models.Models
{
	/// <summary>
	/// Describes a logical display device using absolute physical coordinates
	/// </summary>
	/// <remarks>The <see cref="ConfigurationDefinition"/> objects can use coordinates relative to the display configuration by prepending their name with the name of the display</remarks>
	[JsonObject("displays")]
	public class DisplayDefinition : IDisplayDefinition
	{
		#region Constants

		private const int HASH_START = 17;
		private const int HASH_NUM = 23;

		#endregion Constants

		#region Ctor

		/// <summary>
		/// Standard Ctor
		/// </summary>
		public DisplayDefinition()
		{

		}

		/// <summary>
		/// Copy Ctor
		/// </summary>
		/// <param name="copy"></param>
		public DisplayDefinition(DisplayDefinition copy) : this()
		{
			Name = copy.Name;
			Left = copy.Left;
			Top = copy.Top;
			Width = copy.Width;
			Height = copy.Height;
		}


		#endregion Ctor

		#region Public static Create methods

		/// <summary>
		/// Converts JSON to a <seealso cref="DisplayDefinition"/>
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns><seealso cref="DisplayDefinition"/></returns>
		public static DisplayDefinition FromJson(ILoggerFactory loggerFactory, string jsonString)
		{
			var logger = loggerFactory?.CreateLogger<DisplayDefinition>();
			try
			{
				var settings = new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.All,
					Formatting = Formatting.Indented
				};
				return JsonConvert.DeserializeObject<DisplayDefinition>(jsonString, settings);
			}
			catch (Exception ex)
			{
				logger?.LogError($"Unable to create DisplayDefinition, Exception: {ex}");
			}
			return null;
		}

		#endregion Public static Create methods
		
		#region Display Definitin properties Name, Left, Top, Width, Height

		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("width")]
		public int? Width { get; set; }
		[JsonProperty("height")]
		public int? Height { get; set; }
		[JsonProperty("left")]
		public int? Left { get; set; }
		[JsonProperty("top")]
		public int? Top { get; set; }

		[JsonProperty("offsets")]
		public ImageGeometry ImageGeometry { get; set; }

		#endregion Display Definitin properties Name, Left, Top, Width, Height

		#region Image cropping properties IOffsetGeometry

		/// <summary>
		/// Not really used with <see cref="DisplayDefinition"/>
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
		
		[JsonIgnore()]
		public bool? Center { get; set; }

		#endregion Image cropping properties IOffsetGeometry

		/// <summary>
		/// Centers the current Configuration inside the specified Configuration
		/// </summary>
		/// <param name="configurationDefinition"></param>
		public Point GetCenterTo(IDisplayGeometry displayGeometry)
		{
			var size = new Size(Width ?? 0, Height ?? 0);
			var centerPoint = size.RelativeCenterInRectangle(new Size(displayGeometry?.Width ?? 0, displayGeometry?.Height ?? 0));
			return new Point(centerPoint.X, centerPoint.Y);
		}


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
				hashCode += (Left ?? 0) * HASH_NUM;
				hashCode += (Top ?? 0) * HASH_NUM;
				hashCode += (Width ?? 0) * HASH_NUM;
				hashCode += (Height ?? 0) * HASH_NUM;
				return hashCode;
			}
		}

		/// <summary>
		/// Get the human readable string of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToReadableString();
		}

		/// <summary>
		/// Returns the portion of the readable string specific to <seealso cref="DisplayDefinition"/>
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

		/// <summary>
		/// Returns JSON for <seealso cref="SubConfigurationDefinition"/>
		/// </summary>
		/// <returns></returns>
		public string ToReadableString()
		{
			return $"{Name} ({Left ?? 0}, {Top ?? 0}) ({Width ?? 0}, {Height ?? 0})";
		}

		#endregion Public overrides 

	}
}
