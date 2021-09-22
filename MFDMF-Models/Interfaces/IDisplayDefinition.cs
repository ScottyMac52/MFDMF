namespace MFDMF_Models.Interfaces
{
	using MFDMF_Models.Models;

	public interface IDisplayDefinition : IDisplayGeometry, IReadableObject, INameObject, IOffsetGeometry
	{
		ImageGeometry ImageGeometry { get; set; }
	}
}
