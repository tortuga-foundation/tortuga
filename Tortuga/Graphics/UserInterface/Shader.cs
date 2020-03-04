using System.Drawing;
using System.Numerics;

namespace Tortuga.Graphics.UserInterface
{
    public enum ShadowType
    {
        Outset = 0,
        Inset = 1
    }

    public class Shadow
    {
        public ShadowType Type;
        public Vector2 Offset;
        public float Blur;
        public float Spread;
        public Color Color;
    }
}