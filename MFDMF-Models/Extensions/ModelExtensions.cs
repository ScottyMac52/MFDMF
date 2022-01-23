namespace MFDMF_Models.Extensions
{
	using System.Linq;

	public static class ModelExtensions
	{
		/// <summary>
		/// Better hashcode for strings since Microsoft broke them in .NET Core
		/// </summary>
		/// <param name="source"></param>
		/// <param name="startHash"></param>
		/// <returns></returns>
		public static int ToHashCode(this string source, int? startHash)
		{
			int? x = startHash ?? 0;

			source?.ToList().ForEach((ch) =>
			{
				x += ch;
			});

			return x ?? 0;
		}
	}
}
