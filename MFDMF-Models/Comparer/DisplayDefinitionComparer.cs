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
				result = x.Left.CompareTo(y.Left);
			}
			if (result == 0)
			{
				result = x.Top.CompareTo(y.Top);
			}
			if (result == 0)
			{
				result = x.Width.CompareTo(y.Width);
			}
			if (result == 0)
			{
				result = x.Height.CompareTo(y.Height);
			}

			return result;
		}
	}
}
