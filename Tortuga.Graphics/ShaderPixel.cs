#pragma warning disable 0649

namespace Tortuga.Graphics
{
    /// <summary>
    /// 4 byte pixel
    /// </summary>
    public struct ShaderPixel
    {
        /// <summary>
        /// red
        /// </summary>
        public byte R;
        /// <summary>
        /// green
        /// </summary>
        public byte G;
        /// <summary>
        /// blue
        /// </summary>
        public byte B;
        /// <summary>
        /// alpha
        /// </summary>
        public byte A;

        /// <summary>
        /// constructor for shader pixel
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <param name="a">alpha</param>
        public ShaderPixel(byte r= 0, byte g = 0, byte b = 0, byte a = 255)
        {
            R = r;
            B = b;
            G = g;
            A = a;
        }

        /// <summary>
        /// white color
        /// </summary>
        public static ShaderPixel White 
            => new ShaderPixel(255, 255, 255, 255);
        /// <summary>
        /// black color
        /// </summary>
        public static ShaderPixel Black 
            => new ShaderPixel(0, 0, 0, 255);
        /// <summary>
        /// blue color
        /// </summary>
        public static ShaderPixel Blue
            => new ShaderPixel(0, 0, 255, 255);
    };
}