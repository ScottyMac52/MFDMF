namespace MFDMFApp
{
	using CommandLine;

	public class StartOptions : IStartOptions
	{
		[Option('m', "mod", Required = false, HelpText = "Module to load")]
		public string ModuleName { get; internal set; }
		[Option('s', "submod", Required = false, HelpText = "SubModule to load")]
		public string SubModuleName { get; internal set; }
	}
}