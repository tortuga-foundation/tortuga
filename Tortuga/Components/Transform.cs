using System.Numerics;

namespace Tortuga.Components
{
    /// <summary>
    /// Transform component, used to store position, rotation and scale of an entity
    /// </summary>
    public class Transform : Core.BaseComponent
    {
        /// <summary>
        /// Is the entity static or can the entity change every frame
        /// </summary>
        public bool IsStatic = false;
        /// <summary>
        /// Position of the entity
        /// </summary>
        public Vector3 Position = new Vector3(0, 0, 0);
        /// <summary>
        /// Rotation of the entity
        /// </summary>
        public Quaternion Rotation = Quaternion.Identity;
        /// <summary>
        /// Scale of the entity
        /// </summary>
        public Vector3 Scale = new Vector3(1, 1, 1);

        /// <summary>
        /// Returns model matrix of the entity
        /// </summary>
        public Matrix4x4 Matrix
        {
            get
            {
                var mat = Matrix4x4.Identity;
                mat *= Matrix4x4.CreateScale(Scale);
                mat *= Matrix4x4.CreateFromQuaternion(Rotation);
                mat *= Matrix4x4.CreateTranslation(Position);
                return mat;
            }
            set => Matrix4x4.Decompose(value, out Scale, out Rotation, out Position);
        }
        /// <summary>
        /// Returns right vector of the entity
        /// </summary>
        public Vector3 Right
        {
            get
            {
                var mat = Matrix;
                return Vector3.Normalize(new Vector3(mat.M11, mat.M21, mat.M31));
            }
        }
        /// <summary>
        /// Returns up vector of the entity
        /// </summary>
        public Vector3 Up
        {
            get
            {
                var mat = Matrix;
                return Vector3.Normalize(new Vector3(mat.M12, mat.M22, mat.M32));
            }
        }
        /// <summary>
        /// Returns forward vector of the entity
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                var mat = Matrix;
                return Vector3.Normalize(new Vector3(-mat.M13, -mat.M23, -mat.M33));
            }
        }
    }
}