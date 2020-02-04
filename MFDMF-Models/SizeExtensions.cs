using System.Drawing;

namespace MFDMF_Models.Extensions
{
    public static class SizeExtensions
	{
        public static Point CenterInRectangle(this Size Inner, Rectangle Outer)
        {
            return new Point()
            {
                X = Outer.X + ((Outer.Width - Inner.Width) / 2),
                Y = Outer.Y + ((Outer.Height - Inner.Height) / 2)
            };
        }

        /// <summary>
        /// Assume origin position is (0,0) and give centering coordinates for inner rectangle
        /// </summary>
        /// <param name="Inner">Size of the Inner rectangle</param>
        /// <param name="Outer">Size of the Outer rectangle</param>
        /// <returns></returns>
        public static Point RelativeCenterInRectangle(this Size Inner, Size Outer)
        {
            return new Point()
            {
                X = ((Outer.Width - Inner.Width) / 2),
                Y = ((Outer.Height - Inner.Height) / 2)
            };
        }


    }
}
