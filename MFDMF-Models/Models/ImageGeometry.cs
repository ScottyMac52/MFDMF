namespace MFDMF_Models.Models
{
	using MFDMF_Models.Extensions;
	using MFDMF_Models.Interfaces;
	using Newtonsoft.Json;
	using System.Drawing;

	/// <summary>
	/// Encapsulartes an object that defines a geometric area
	/// </summary>
	public class ImageGeometry : IDisplayGeometry
	{
		[JsonProperty("width")]
		public int? Width { get; set; }
		[JsonProperty("height")]
		public int? Height { get; set; }
		[JsonProperty("left")]
		public int? Left { get; set; }
		[JsonProperty("top")]
		public int? Top { get; set; }
		[JsonIgnore()]
		public bool? Center { get; set; }

		public Point GetCenterTo(IDisplayGeometry displayGeometry)
		{
			var size = new Size(Width ?? 0, Height ?? 0);
			var centerPoint = size.RelativeCenterInRectangle(new Size(displayGeometry?.Width ?? 0, displayGeometry?.Height ?? 0));
			return new Point(centerPoint.X, centerPoint.Y);
		}
	}
}
