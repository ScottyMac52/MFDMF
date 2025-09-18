namespace MFDMF_Models.Models
{
	using MFDMF_Models.Interfaces;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;

	public class AppSettings : IEquatable<AppSettings>
	{
		public AppSettings()
		{

		}

		public AppSettings(AppSettings source)
		{
			DisplayConfigurationFile = source.DisplayConfigurationFile;
			DcsSavedGamesPath = source.DcsSavedGamesPath;
			ShowTooltips = source.ShowTooltips;
			UseCougar = source.UseCougar;
			SaveCroppedImages = source.SaveCroppedImages;
			DefaultConfiguration = source.DefaultConfiguration;
			FilePath = source.FilePath;
			FileSpec = source.FileSpec;
			TurnOffCache = source.TurnOffCache;
			ShowRulers = source.ShowRulers;
			RulerSize = source.RulerSize;
			CreateKneeboard = source.CreateKneeboard;
			ModuleItems = source.ModuleItems;
		}

		/// <summary>
		/// Defines this users display geometry
		/// </summary>
		[JsonProperty("displayConfigurationFile")]
		public string DisplayConfigurationFile { get; set; }

		/// <summary>
		/// Name of folder from Saved Games where the DCS Saved Game files are located, defaults to DCS
		/// </summary>
		[JsonProperty("dcsSavedGamesPath")]
		public string DcsSavedGamesPath { get; set; } = "DCS";

		/// <summary>
		/// Displays a helpful tooltip when you hover over one of the images
		/// </summary>
		[JsonProperty("showTooltips")]
		public bool? ShowTooltips { get; set; }

		/// <summary>
		/// If true then Cougar otherwise Warthog
		/// </summary>
		[JsonProperty("useCougar")]
		public bool? UseCougar { get; set; }

		/// <summary>
		/// Save the cropped images?
		/// </summary>
		[JsonProperty("saveCroppedImages")]
		public bool? SaveCroppedImages { get; set; }

		/// <summary>
		/// Default module to load on startup if not passed on command line
		/// </summary>
		[JsonProperty("defaultConfiguration")]
		public string DefaultConfiguration { get; set; }

		/// <summary>
		/// The root path for all the images
		/// </summary>
		[JsonProperty("filePath")]
		public string FilePath { get; set; }

		/// <summary>
		/// The files to load (JSON)
		/// </summary>
		[JsonProperty("fileSpec")]
		public string FileSpec { get; set; }

		/// <summary>
		/// The modules that are currently loaded
		/// </summary>
		[JsonIgnore]
		public IEnumerable<IModuleDefinition> ModuleItems { get; set; }

		/// <summary>
		/// If true then the cache is off and all files are generated
		/// </summary>
		[JsonProperty("turnOffCache")]
		public bool? TurnOffCache { get; set; }

		/// <summary>
		/// If true then the X/Y rulers are displayed on the rendered images
		/// </summary>
		[JsonProperty("showRulers")]
		public bool? ShowRulers { get; set; }

		/// <summary>
		/// The length of the major divisions on the ruler lines
		/// </summary>
		[JsonProperty("rulerSize")]
		public int? RulerSize { get; set; }

		/// <summary>
		/// If true then a kneeboard file is created for each loaded profiles loaded configuration
		/// </summary>
		[JsonProperty("createKneeboard")]
		public bool? CreateKneeboard { get; set; }

        [JsonProperty("mainWindowLeft")]
        public double? MainWindowLeft { get; set; }

        [JsonProperty("mainWindowTop")]
        public double? MainWindowTop { get; set; }

        public override string ToString()
		{
			var cachingStatus = (TurnOffCache ?? false) ? "No" : "Yes";
			var rulerStatus = (ShowRulers ?? false) ? $"Yes: {RulerSize ?? 0}px" : "No";
			var throttleType = (UseCougar ?? false) ? "Cougar" : "Warthog";
			return $"Display Configuration: {DisplayConfigurationFile} Throttle: {throttleType} Show tooltips: {ShowTooltips ?? false} Show rulers: {rulerStatus} Save Crops: {SaveCroppedImages ?? false} File path: {FilePath} File Spec: {FileSpec} Caching: {cachingStatus}";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AppSettings);
		}

		public bool Equals(AppSettings other)
		{
			return other != null &&
				   DisplayConfigurationFile == other.DisplayConfigurationFile &&
				   DcsSavedGamesPath == other.DcsSavedGamesPath &&
				   ShowTooltips == other.ShowTooltips &&
				   UseCougar == other.UseCougar &&
				   SaveCroppedImages == other.SaveCroppedImages &&
				   DefaultConfiguration == other.DefaultConfiguration &&
				   FilePath == other.FilePath &&
				   FileSpec == other.FileSpec &&
				   EqualityComparer<IEnumerable<IModuleDefinition>>.Default.Equals(ModuleItems, other.ModuleItems) &&
				   TurnOffCache == other.TurnOffCache &&
				   ShowRulers == other.ShowRulers &&
				   RulerSize == other.RulerSize &&
				   CreateKneeboard == other.CreateKneeboard;
		}

		public override int GetHashCode()
		{
			System.HashCode hash = new System.HashCode();
			hash.Add(DisplayConfigurationFile);
			hash.Add(DcsSavedGamesPath);
			hash.Add(ShowTooltips);
			hash.Add(UseCougar);
			hash.Add(SaveCroppedImages);
			hash.Add(DefaultConfiguration);
			hash.Add(FilePath);
			hash.Add(FileSpec);
			hash.Add(ModuleItems);
			hash.Add(TurnOffCache);
			hash.Add(ShowRulers);
			hash.Add(RulerSize);
			hash.Add(CreateKneeboard);
			return hash.ToHashCode();
		}
	}
}
