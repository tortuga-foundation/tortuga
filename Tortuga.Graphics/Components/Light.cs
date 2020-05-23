using System.Drawing;
using System.Numerics;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Light component, used to create lighting
    /// </summary>
    public class Light : Core.BaseComponent
    {
        /// <summary>
        /// Different types of light
        /// </summary>
        public enum LightType
        {
            /// <summary>
            /// Point light
            /// </summary>
            Point = 0,
            /// <summary>
            /// Directional light
            /// </summary>
            Directional = 1
        }

        /// <summary>
        /// Color of the light
        /// </summary>
        public Color Color;
        /// <summary>
        /// Type of light
        /// </summary>
        public LightType Type;
        /// <summary>
        /// Intensity of the light
        /// </summary>
        public float Intensity;

        /// <summary>
        /// Forward direction of the light
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return new Vector3(1, 0, 0);

                return transform.Forward;
            }
        }
        /// <summary>
        /// Position of the light
        /// </summary>
        public Vector3 Position
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return new Vector3(0, 0, 0);

                return transform.Position;
            }
        }

        internal LightShaderInfo BuildShaderInfo
            => new LightShaderInfo
            {
                Type = (int)Type,
                Color = new Vector4(
                    Color.R / 255, 
                    Color.G / 255, 
                    Color.B / 255, 
                    Color.A / 255
                ),
                Forward = new Vector4(Forward, 1.0f),
                Intensity = Intensity,
                Position = new Vector4(Position, 1.0f),
            };

#pragma warning disable 0649
        internal struct LightShaderInfo
        {
            public Vector4 Position;
            public Vector4 Forward;
            public Vector4 Color;
            public int Type;
            public float Intensity;
            public int Reserved1;
            public int Reserved2;
        }
        internal struct FullShaderInfo
        {
            public LightShaderInfo Light0;
            public LightShaderInfo Light1;
            public LightShaderInfo Light2;
            public LightShaderInfo Light3;
            public LightShaderInfo Light4;
            public LightShaderInfo Light5;
            public LightShaderInfo Light6;
            public LightShaderInfo Light7;
            public LightShaderInfo Light8;
            public LightShaderInfo Light9;
            public int Count;
        }
#pragma warning restore 0649
    }
}