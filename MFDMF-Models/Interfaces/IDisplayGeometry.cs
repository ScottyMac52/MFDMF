using System.Drawing;

namespace MFDMF_Models.Interfaces
{
    public interface IDisplayGeometry
	{
        /// <summary>
        /// Width of the displayed image
        /// </summary>
        int? Width { get; set; }
        /// <summary>
        /// The Height of the displayed image
        /// </summary>
        int? Height { get; set; }
        /// <summary>
        /// Left coordinate of the displayed image
        /// </summary>
        int? Left { get; set; }
        /// <summary>
        /// Top coordinate of the displayed image
        /// </summary>
        int? Top { get; set; }

        /// <summary>
        /// Gets the center coordinates for the current display inside the specified display
        /// </summary>
        /// <param name="displayGeometry"></param>
        /// <returns></returns>
        Point GetCenterTo(IDisplayGeometry displayGeometry);
    }
}
