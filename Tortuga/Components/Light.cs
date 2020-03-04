using System.Drawing;
using System.Numerics;

namespace Tortuga.Components
{
    public class Light : Core.BaseComponent
    {
        public enum LightType
        {
            Point = 0,
            Directional = 1
        }

        public Color Color;
        public LightType Type;
        public float Intensity;

        public Vector3 Forward
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return new Vector3(1, 0, 0);

                return transform.Forward;
            }
        }
        public Vector3 Position
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return new Vector3(0, 0, 0);

                return transform.Position;
            }
        }

#pragma warning disable 0649
        public struct LightShaderInfo
        {
            public Vector4 Position;
            public Vector4 Forward;
            public Vector4 Color;
            public int Type;
            public float Intensity;
            public int Reserved1;
            public int Reserved2;
        }
        public struct FullShaderInfo
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