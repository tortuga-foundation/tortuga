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

        private Matrix4x4 TranslateMatrix(Matrix4x4 matrix, Vector3 vector)
        {
            matrix.M41 += vector.X;
            matrix.M42 += vector.Y;
            matrix.M43 += vector.Z;
            return matrix;
        }

        private Matrix4x4 RotateMatrix(Matrix4x4 matrix, Quaternion rotation)
        {
            float sqW = rotation.W * rotation.W;
            float sqX = rotation.X * rotation.X;
            float sqY = rotation.Y * rotation.Y;
            float sqZ = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float xz = rotation.X * rotation.Z;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            matrix.M11 -= (2 * sqY) - (2 * sqZ);
            matrix.M21 = (2 * xy) - (2 * zw);
            matrix.M31 = (2 * xz) + (2 * yw);
            
            matrix.M12 = (2 * xy) + (2 * zw);
            matrix.M22 -= (2 * sqX) - (2 * sqZ);
            matrix.M32 = (2 * yz) - (2 * xw);
            
            matrix.M13 = (2 * xz) - (2 * yw);
            matrix.M23 = (2 * yz) + (2 * xw);
            matrix.M33 -= (2 * sqX) - (2 * sqY);

            return matrix;
        }

        private Matrix4x4 ScaleMatrix(Matrix4x4 matrix, Vector3 scale)
        {
            matrix.M11 *= scale.X;
            matrix.M22 *= scale.Y;
            matrix.M33 *= scale.Z;
            return matrix;
        }

        /// <summary>
        /// Returns model matrix of the entity
        /// </summary>
        public Matrix4x4 Matrix
        {
            get
            {
                var mat = Matrix4x4.Identity;
                mat = ScaleMatrix(mat, Scale);
                mat = RotateMatrix(mat, Rotation);
                mat = TranslateMatrix(mat, Position);
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