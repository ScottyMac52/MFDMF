using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using MFDMF_Services.Displays;
using System;
using System.Collections.Generic;

namespace XUnitTestProject_MFDMF.Mocks
{
	public class MockDisplayConfigurationLoader : IDisplayConfigurationService
	{
		private Func<List<IDisplayDefinition>> _createTestDataMethod;

		public MockDisplayConfigurationLoader(Func<List<IDisplayDefinition>> createTestDataMethod)
		{
			_createTestDataMethod = createTestDataMethod;
		}
		
		public List<IDisplayDefinition> LoadDisplays()
		{
			return _createTestDataMethod?.Invoke();
		}

		public bool SaveDisplayConfigurations(List<DisplayDefinition> displayDefinitions)
		{
			return true;
		}
	}
}
