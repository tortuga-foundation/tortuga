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
        public float Range;

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
    }
}