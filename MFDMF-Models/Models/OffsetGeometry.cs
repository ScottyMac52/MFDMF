using MFDMF_Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MFDMF_Models.Models
{
	public class OffsetGeometry : IOffsetGeometry
	{
		public float? Opacity { get; set; }
		public int? XOffsetStart { get; set; }
		public int? YOffsetStart { get; set; }
		public int? XOffsetFinish { get; set; }
		public int? YOffsetFinish { get; set; }
	}
}
