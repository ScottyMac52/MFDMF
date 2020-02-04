using MFDMF_Models.Interfaces;
using System.Collections.Generic;

namespace MFDMF_Services.Displays
{
	public interface IDisplayConfigurationService
	{
		List<IDisplayDefinition> LoadDisplays();
}
}
