using System.Drawing;
using System.Numerics;

namespace Tortuga.Components
{
    public class UserInterface : Core.BaseComponent
    {
        public enum ShadowType
        {
            Outset = 0,
            Inset = 1
        }

        public class BoxShadow
        {
            public ShadowType Type;
            public Vector2 Offset;
            public float Blur;
            public float Spread;
            public Color Color;
        }

        public Vector2 PositionPixel;

        public Vector2 ScalePixel;

        public bool IsStatic;
        public int IndexZ = 0;
        public float BorderRadius = 0;
        public Graphics.Image Background;
        public BoxShadow Shadow;

        internal struct ShaderUIStruct
        {
            public Vector2 Position;
            public Vector2 Scale;
            public int IsStatic;
            public int IndexZ;
            public float BorderRadius;
            public int ShadowType;
            public Vector2 ShadowOffset;
            public float ShadowBlur;
            public float ShadowSpread;
            public Vector4 ShadowColor;
        }

        internal ShaderUIStruct BuildShaderStruct
            => new ShaderUIStruct
            {
                Position = PositionPixel,
                Scale = ScalePixel,
                IsStatic = IsStatic ? 1 : 0,
                IndexZ = IndexZ,
                BorderRadius = BorderRadius,
                ShadowType = Shadow.Type == ShadowType.Outset ? 0 : 1,
                ShadowOffset = Shadow.Offset,
                ShadowBlur = Shadow.Blur,
                ShadowSpread = Shadow.Spread,
                ShadowColor = new Vector4(Shadow.Color.R, Shadow.Color.G, Shadow.Color.B, Shadow.Color.A)
            };
    }
}