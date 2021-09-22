namespace MFDMF_Services.Displays
{
	using MFDMF_Models.Interfaces;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public interface IDisplayConfigurationService
	{
		Task<IEnumerable<IDisplayDefinition>> LoadDisplaysAsync();
	}
}
