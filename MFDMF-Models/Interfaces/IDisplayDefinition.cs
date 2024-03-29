﻿using MFDMF_Models.Models;
using Newtonsoft.Json;

namespace MFDMF_Models.Interfaces
{
	public interface IDisplayDefinition : IDisplayGeometry, IReadableObject, INameObject, IOffsetGeometry
	{
		/// <summary>
		/// 
		/// </summary>
		ImageGeometry ImageGeometry { get; set; }

		/// <summary>
		/// If true then any configurations created in this <see cref="IDisplayDefinition"/> will be set to always on top
		/// </summary>
		[JsonProperty("alwaysOnTop")]
		bool? AlwaysOnTop { get; set; }

		/// <summary>
		/// If false then disabled 
		/// </summary>
		/// <remarks>Should be implemented as true by default</remarks>
		[JsonProperty("enabled")]
		bool Enabled { get; set; }
	}
}
