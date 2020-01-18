using MFDMF_Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MFDMF_Services.Displays
{
	public interface IDisplayConfigurationService
	{
		List<DisplayDefinition> LoadDisplays();
}
}
