using System.Numerics;

namespace Tortuga.Components
{
    public class Transform : Core.BaseComponent
    {
        public bool IsStatic = false;
        public Vector3 Position = new Vector3(0, 0, 0);
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = new Vector3(1, 1, 1);
    }
}