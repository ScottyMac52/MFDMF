using System.Threading.Tasks;

namespace MFDMF_Services.Configuration
{
	public interface IModelLoaderService
	{
		Task<T> LoadJsonFileAsync<T>(string jsonFile);
	}
}