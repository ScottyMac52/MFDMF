namespace MFDMF_Models.Interfaces
{
	public interface IImagePath
	{
		string FileName { get; set; }

		string FilePath { get; set; }

		bool? Enabled { get; set; }
	}
}
