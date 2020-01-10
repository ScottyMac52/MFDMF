using MFDMF_Models.Interfaces;

namespace MFDMF_Services.Configuration
{
	public interface IConfigurationLoadingService
	{
		IMFDMFDefinition LoadConfiguration();
	}
}