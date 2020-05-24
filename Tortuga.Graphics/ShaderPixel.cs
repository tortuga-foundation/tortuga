#pragma warning disable 0649

namespace Tortuga.Graphics
{
    internal struct ShaderPixel
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public ShaderPixel(byte r= 0, byte g = 0, byte b = 0, byte a = 1)
        {
            R = r;
            B = b;
            G = g;
            A = a;
        }
    };
}