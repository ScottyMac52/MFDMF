namespace MFDMF_Models.Models
{
	using MFDMF_Models.Extensions;
	using MFDMF_Models.Interfaces;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using System;
	using System.Drawing;

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
		public DisplayDefinition(IDisplayDefinition copy) : this()
		{
			Name = copy.Name;
			Left = copy.Left;
			Top = copy.Top;
			Width = copy.Width;
			Height = copy.Height;
			AlwaysOnTop = copy.AlwaysOnTop;
			Enabled = copy.Enabled;
		}

		public DisplayDefinition(IConfigurationDefinition copy)
		{
			Name = copy.Name;
			Enabled = copy.Enabled ?? true;
			Left = copy.Left;
			Top = copy.Top;
			Width = copy.Width;
			Height = copy.Height;
			XOffsetStart = copy.XOffsetStart;
			XOffsetFinish = copy.XOffsetFinish;
			YOffsetStart = copy.YOffsetStart;
			YOffsetFinish = copy.YOffsetFinish;
			UseAsSwitch = copy.UseAsSwitch;
			Opacity = copy.Opacity;
			Center = copy.Center;
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

        /// <inheritdoc/>
		[JsonProperty("opacity")]
		public float? Opacity { get; set; }

        /// <inheritdoc/>
		[JsonProperty("xOffsetStart")]
		public int? XOffsetStart { get; set; }

        /// <inheritdoc/>
		[JsonProperty("yOffsetStart")]
		public int? YOffsetStart { get; set; }

        /// <inheritdoc/>
		[JsonProperty("xOffsetFinish")]
		public int? XOffsetFinish { get; set; }

        /// <inheritdoc/>
        [JsonProperty("yOffsetFinish")]
		public int? YOffsetFinish { get; set; }

        /// <inheritdoc/>
        [JsonIgnore()]
		public bool? Center { get; set; }

        /// <inheritdoc/>
        public bool? MakeOpaque { get; set; }

        /// <inheritdoc/>
        [JsonIgnore()]
		public Point CroppingStart => this.YOffsetStart.HasValue && this.YOffsetStart.HasValue ? new Point(this.XOffsetStart.Value, this.YOffsetStart.Value) : new Point();

        /// <inheritdoc/>
        [JsonIgnore()]
		public Rectangle CroppingArea => this.XOffsetStart.HasValue && this.XOffsetFinish.HasValue && this.YOffsetStart.HasValue && this.YOffsetFinish.HasValue ? new Rectangle(CroppingStart, new Size(CroppedWidth, CroppedHeight)) : new Rectangle();
        
		/// <inheritdoc/>
        [JsonIgnore()]
		public int CroppedWidth => this.XOffsetStart.HasValue && this.XOffsetFinish.HasValue ? Math.Abs(this.XOffsetFinish.Value - this.XOffsetStart.Value) : 0;

        /// <inheritdoc/>
        [JsonIgnore()]
		public int CroppedHeight => this.YOffsetStart.HasValue && this.YOffsetFinish.HasValue ? Math.Abs(this.YOffsetFinish.Value - this.YOffsetStart.Value) : 0;

        /// <inheritdoc/>
        [JsonProperty("alwaysOnTop")]
		public bool? AlwaysOnTop { get; set; }

		/// <inheritdoc/>
		[JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

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
				hashCode += (Center?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Height?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Left?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Name?.ToHashCode(HASH_START) ?? 0) * HASH_NUM;
				hashCode += Enabled.GetHashCode() * HASH_NUM;
				hashCode += (AlwaysOnTop?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Opacity?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Top?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (UseAsSwitch?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (Width?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (XOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (XOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (YOffsetFinish?.GetHashCode() ?? 0) * HASH_NUM;
				hashCode += (YOffsetStart?.GetHashCode() ?? 0) * HASH_NUM;
				return hashCode;
			}
		}

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
			return $"{Enabled} - {Name} ({Left ?? 0}, {Top ?? 0}) ({Width ?? 0}, {Height ?? 0})";
		}

		#endregion Public overrides 

	}
}
