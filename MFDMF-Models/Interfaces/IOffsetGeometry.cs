using System.Drawing;

namespace MFDMF_Models.Interfaces
{
    public interface IOffsetGeometry
	{
        /// <summary>
        /// Used for Subconfigurations to designate them as switchable images
        /// </summary>
        bool? UseAsSwitch { get; set; }
        /// <summary>
        /// The percentage of Opacity for the object
        /// </summary>
        float? Opacity { get; set; }
        /// <summary>
        /// Starting X position of the Crop
        /// </summary>
        int? XOffsetStart { get; set; }
        /// <summary>
        /// Starting Y position of the Crop
        /// </summary>
        int? YOffsetStart { get; set; }
        /// <summary>
        /// Ending X position of the Crop
        /// </summary>
        int? XOffsetFinish { get; set; }
        /// <summary>
        /// Ending Y position of the Crop
        /// </summary>
        int? YOffsetFinish { get; set; }

        /// <summary>
        /// Starting coordinate of the crop
        /// </summary>
        Point CroppingStart { get; }

        /// <summary>
        /// Gets the Cropped area represented by the configuration
        /// </summary>
        Rectangle CroppingArea { get; }

        /// <summary>
        /// The Width of the Cropped Area
        /// </summary>
        int CroppedWidth { get; }

        /// <summary>
        /// The Height of the Cropped Area
        /// </summary>
        int CroppedHeight { get; }

    }
}
