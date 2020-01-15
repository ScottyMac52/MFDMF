using Newtonsoft.Json;

namespace MFDMF_Models.Models.TestPattern
{
	public class TestPatternDefinition : ConfigurationDefinition
	{
		/// <summary>
		/// The image bytes for this test pattern
		/// </summary>
		public byte[] ImageBytes { get; set; }

		[JsonProperty("pattern")]
		public string Pattern { get; set; }

		public TestPatternDefinition()  : base()
		{
		}

		public TestPatternDefinition(ConfigurationDefinition source) : base(source)
		{
		}

		public override string ToReadableString()
		{
			return $"{base.ToReadableString()} TestPattern: {Pattern} {ImageBytes?.Length ?? 0} bytes";
		}
	}
}
