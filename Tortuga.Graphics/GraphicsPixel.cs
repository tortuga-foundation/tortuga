using System.Drawing;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Graphics Color Mapper
    /// </summary>
    public static class GraphicsColorMapper
    {
        /// <summary>
        /// System.Drawing.Color to GraphicsColor mapper
        /// </summary>
        /// <param name="color">System.Drawing.Color</param>
        /// <returns>GraphicsColor</returns>
        public static GraphicsColor ToGraphicsColor(this Color color)
        => new GraphicsColor(
            color.R,
            color.G,
            color.B,
            color.A
        );
    }

    /// <summary>
    /// A single color information that is used by the gpu
    /// </summary>
    public struct GraphicsColor
    {
        /// <summary>
        /// R
        /// </summary>
        public byte R;
        /// <summary>
        /// G
        /// </summary>
        public byte G;
        /// <summary>
        /// B
        /// </summary>
        public byte B;
        /// <summary>
        /// A
        /// </summary>
        public byte A;

        /// <summary>
        /// constructor for graphics color
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <param name="a">alpha</param>
        public GraphicsColor(
            byte r = 255,
            byte g = 255,
            byte b = 255,
            byte a = 255
        )
        {
            R = r;
            B = b;
            G = g;
            A = a;
        }
    }
}