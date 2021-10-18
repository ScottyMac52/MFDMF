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
	public class DisplayDefinition : ConfigurationDefinition
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
			XOffsetStart = copy.XOffsetStart;
			XOffsetFinish = copy.XOffsetFinish;
			YOffsetStart = copy.YOffsetStart;
			YOffsetFinish = copy.YOffsetFinish;
			UseAsSwitch = copy.UseAsSwitch;
			MakeOpaque = copy.MakeOpaque;
			Opacity = copy.Opacity;
			Center = copy.Center;
		}

		public DisplayDefinition(IConfigurationDefinition copy)
		{
			Name = copy.Name;
			Left = copy.Left;
			Top = copy.Top;
			Width = copy.Width;
			Height = copy.Height;
			XOffsetStart = copy.XOffsetStart;
			XOffsetFinish = copy.XOffsetFinish;
			YOffsetStart = copy.YOffsetStart;
			YOffsetFinish = copy.YOffsetFinish;
			UseAsSwitch = copy.UseAsSwitch;
			MakeOpaque = copy.MakeOpaque;
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
		/// Get the human readable string of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToReadableString();
		}

		#endregion Public overrides 

	}
}
