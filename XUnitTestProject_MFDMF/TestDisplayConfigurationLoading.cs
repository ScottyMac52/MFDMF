using MFDMF_Models.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using XUnitTestProject_MFDMF.Mocks;

namespace XUnitTestProject_MFDMF
{
	public class TestDisplayConfigurationLoading : BaseTesting
	{
		private List<DisplayDefinition> _displayList;

		public TestDisplayConfigurationLoading()
		{
			_displayList = new List<DisplayDefinition>()
				{
					new DisplayDefinition()
					{
						Name = "Main",
						Left = 0,
						Top = 0,
						Width = 2560,
						Height = 1440
					},
					new DisplayDefinition()
					{
						Name = "LMFD",
						Left = -1280,
						Top = 0,
						Width = 1280,
						Height = 720
					},
					new DisplayDefinition()
					{
						Name = "WHKEY",
						Left = 2561,
						Top = 0,
						Width = 1920,
						Height = 1080
					},
					new DisplayDefinition()
					{
						Name = "RMFD",
						Left = 2561 + 1280,
						Top = 0,
						Width = 1280,
						Height = 720
					}
				};
		}


		[Fact]
		public void Test_Model_ToJson_FromJson()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.True(AssertAreEqual(displayList, testDisplayList));
		}

		[Fact]
		public void Test_When_Model_Changes_Equality_Changes_Name()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			displayList[0].Name = "Changed!";
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.False(AssertAreEqual(displayList, testDisplayList));
		}

		[Fact]
		public void Test_When_Model_Changes_Equality_Changes_Left()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			displayList[1].Left = 37612453;
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.False(AssertAreEqual(displayList, testDisplayList));
		}

		[Fact]
		public void Test_When_Model_Changes_Equality_Changes_Top()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			displayList[2].Top = 37612453;
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.False(AssertAreEqual(displayList, testDisplayList));
		}

		[Fact]
		public void Test_When_Model_Changes_Equality_Changes_Width()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			displayList[3].Width = 37612453;
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.False(AssertAreEqual(displayList, testDisplayList));
		}

		[Fact]
		public void Test_When_Model_Changes_Equality_Changes_Height()
		{
			// ARRANGE
			var displayLoadingService = new MockDisplayConfigurationLoader(() =>
			{
				return _displayList;
			});

			// ACT
			var displayList = displayLoadingService.LoadDisplays();
			var serializedDisplayList = JsonConvert.SerializeObject(displayList);
			displayList[0].Height = 37612453;
			var testDisplayList = JsonConvert.DeserializeObject<List<DisplayDefinition>>(serializedDisplayList);

			// ASSERT
			Assert.False(AssertAreEqual(displayList, testDisplayList));
		}


		private bool AssertAreEqual(List<DisplayDefinition> disp1, List<DisplayDefinition> disp2)
		{
			bool result = false;
			var listProcessed = new List<string>();
			result = disp1?.Count.Equals(disp2?.Count) ?? false;
			if (result)
			{
				disp1?.ForEach(displaySource =>
				{
					if (result)
					{
						var displayCompare = disp2?.FirstOrDefault(disp => disp?.Name?.Equals(displaySource.Name) ?? false);
						result = displaySource?.Equals(displayCompare) ?? false;
						listProcessed.Add(displayCompare?.Name);
					}
				});

				if (disp2.Any(display => !listProcessed.Contains(display.Name)))
				{
					return false;
				}
			}

			return result;
		}
	}
}
