using System;
using System.Collections.Generic;
using System.Text;

namespace MFDMF_Models.Interfaces
{
	public interface IOffsetGeometry
	{
        /// <summary>
        /// The percentage of Opacity for the object
        /// </summary>
        float Opacity { get; set; }
        /// <summary>
        /// Starting X position of the Crop
        /// </summary>
        int XOffsetStart { get; set; }
        /// <summary>
        /// Starting Y position of the Crop
        /// </summary>
        int YOffsetStart { get; set; }
        /// <summary>
        /// Ending X position of the Crop
        /// </summary>
        int XOffsetFinish { get; set; }
        /// <summary>
        /// Ending Y position of the Crop
        /// </summary>
        int YOffsetFinish { get; set; }
    }
}
