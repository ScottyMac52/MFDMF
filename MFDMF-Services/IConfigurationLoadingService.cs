using MFDMF_Models.Interfaces;

namespace MFDMF_Services
{
	public interface IConfigurationLoadingService
	{
		IMFDMFDefinition LoadConfiguration();
	}
}