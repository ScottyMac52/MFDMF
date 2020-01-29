using MFDMF_Models.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MFDMF_Models.Comparer
{
	public class DisplayDefinitionComparer : IComparer<DisplayDefinition>
	{
		public int Compare([AllowNull] DisplayDefinition x, [AllowNull] DisplayDefinition y)
		{
			int result = x.Name.CompareTo(y.Name);
			if(result == 0)
			{
				result = x?.Left?.CompareTo(y?.Left ?? 0) ?? 0;
			}
			if (result == 0)
			{
				result = x?.Top?.CompareTo(y?.Top ?? 0) ?? 0;
			}
			if (result == 0)
			{
				result = x?.Width?.CompareTo(y?.Width ?? 0) ?? 0;
			}
			if (result == 0)
			{
				result = x?.Height?.CompareTo(y?.Height ?? 0) ?? 0;
			}

			return result;
		}
	}
}
