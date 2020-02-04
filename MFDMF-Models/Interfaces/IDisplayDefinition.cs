using MFDMF_Models.Models;

namespace MFDMF_Models.Interfaces
{
	public interface IDisplayDefinition : IDisplayGeometry, IReadableObject, INameObject, IOffsetGeometry
	{
		ImageGeometry ImageGeometry { get; set; }
	}
}
