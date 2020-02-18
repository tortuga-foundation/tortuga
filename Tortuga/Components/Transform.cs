using System.Numerics;

namespace Tortuga.Components
{
    public class Transform : Core.BaseComponent
    {
        public bool IsStatic = false;
        public Vector3 Position = new Vector3(0, 0, 0);
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = new Vector3(1, 1, 1);


        public Matrix4x4 ToMatrix
        {
            get
            {
                var mat = Matrix4x4.Identity;
                mat *= Matrix4x4.CreateScale(Scale);
                mat *= Matrix4x4.CreateFromQuaternion(Rotation);
                mat *= Matrix4x4.CreateTranslation(Position);
                return mat;
            }
        }
        public Vector3 Right
        {
            get
            {
                var mat = ToMatrix;
                return Vector3.Normalize(new Vector3(mat.M11, mat.M21, mat.M31));
            }
        }
        public Vector3 Up
        {
            get
            {
                var mat = ToMatrix;
                return Vector3.Normalize(new Vector3(mat.M12, mat.M22, mat.M32));
            }
        }
        public Vector3 Forward
        {
            get
            {
                var mat = ToMatrix;
                return Vector3.Normalize(new Vector3(-mat.M13, -mat.M23, -mat.M33));
            }
        }
    }
}