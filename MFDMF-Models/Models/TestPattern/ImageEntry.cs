using MFDMF_Models.Interfaces;

namespace MFDMF_Models.Models.TestPattern
{
	/// <summary>
	/// Image entry 
	/// </summary>
	public class ImageEntry : INameObject, IDisplayGeometry, IImageContent
	{
		public byte[] ImageBytes { get; set; }
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Left { get; set; }
		public int Top { get; set; }
	}
}
