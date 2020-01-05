using System;
using System.Collections.Generic;
using System.Text;

namespace MFDMF_Models.Interfaces
{
	public interface IInsetDisplayGeometry
	{
        /// <summary>
        /// The X coordinate inside of the Configuration image relative to (0,0) to start rendering the image
        /// </summary>
        int StartX { get; set; }
        /// <summary>
        /// The X coordinate inside of the Configuration image relative to (0,0) to end rendering the image
        /// </summary>
        int EndX { get; set; }
        /// <summary>
        /// The Y coordinate inside of the Configuration image relative to (0,0) to start rendering the image
        /// </summary>
        int StartY { get; set; }
        /// <summary>
        /// The Y coordinate inside of the Configuration image relative to (0,0) to end rendering the image
        /// </summary>
        int EndY { get; set; }
    }
}
