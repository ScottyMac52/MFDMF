namespace MFDMF_Services
{
	using MFDMF_Models;
	using MFDMF_Models.Interfaces;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Threading.Tasks;

	public interface IConfigurationProvider
	{
		/// <summary>
		/// Loads all of the modules based on the fileSpec from the specified Path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileSpec"></param>
		/// <returns></returns>
		Task<IEnumerable<IModuleDefinition>> GetModulesAsync(string path, string fileSpec);

		/// <summary>
		/// Display defintions that are defined 
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDisplayDefinition> DisplayDefinitions { get; }

		/// <summary>
		/// Loads all of a module's images 
		/// </summary>
		/// <param name="module"></param>
		/// <param name="throttleKey"></param>
		/// <param name="hotasKey"></param>
		/// <param name="forceReload"></param>
		/// <returns></returns>
		Dictionary<string, ImageDefinition> LoadModuleImages(IModuleDefinition module, string throttleKey, string hotasKey, bool forceReload = false);

		/// <summary>
		/// Loads all the images for a <see cref="IConfigurationDefinition"/>
		/// </summary>
		/// <param name="module"></param>
		/// <param name="config"></param>
		/// <param name="throttleKey"></param>
		/// <param name="hotasKey"></param>
		/// <param name="forceReload"></param>
		/// <returns></returns>
		Dictionary<string, ImageDefinition> LoadConfigurationImages(IModuleDefinition module, IConfigurationDefinition config, string throttleKey, string hotasKey, bool forceReload = false);

		/// <summary>
		/// Reloads ALL cache files for all modules in the path and all configurations and sub-configurations 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="throttleKey"></param>
		/// <param name="hotasKey"></param>
		/// <param name="loadKneeboards"></param>
		/// 
		Task<Dictionary<string, ImageDefinition>> ReloadCacheForAllModulesAsync(string path, string throttleKey, string hotasKey, bool loadKneeboards = false);
	}
}