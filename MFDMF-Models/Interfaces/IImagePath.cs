using System;
using System.Collections.Generic;
using System.Text;

namespace MFDMF_Models.Interfaces
{
	public interface IImagePath
	{
		string FileName { get; set; }

		string FilePath { get; set; }

		bool? Enabled { get; set; }
	}
}
