using System.Collections.Generic;
using System.Numerics;

namespace Tortuga.Core
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
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = value;
                RecalculateInstancedData();
                RecalculateMatrix();
            }
        }
        /// <summary>
        /// Rotation of the entity
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value)
                    return;

                _rotation = value;
                RecalculateInstancedData();
                RecalculateMatrix();
            }
        }
        /// <summary>
        /// Scale of the entity
        /// </summary>
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;

                _scale = value;
                RecalculateInstancedData();
                RecalculateMatrix();
            }
        }

        /// <summary>
        /// Returns model matrix of the entity
        /// </summary>
        public Matrix4x4 Matrix => _matrix;
        /// <summary>
        /// Returns right vector of the entity
        /// </summary>
        public Vector3 Right
        {
            get
            {
                var mat = Matrix;
                return Vector3.Normalize(new Vector3(mat.M11, mat.M12, mat.M13));
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
                return Vector3.Normalize(new Vector3(mat.M21, mat.M22, mat.M23));
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
                return Vector3.Normalize(new Vector3(mat.M31, mat.M32, mat.M33));
            }
        }

        /// <summary>
        /// returns instanced data (position, rotation, scale) in a byte array
        /// </summary>
        public byte[] InstancedData => _instancedData;

        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;
        private byte[] _instancedData;
        private Matrix4x4 _matrix;

        /// <summary>
        /// constructor for transform
        /// </summary>
        public Transform()
        {
            RecalculateInstancedData();
            RecalculateMatrix();
        }

        private void RecalculateInstancedData()
        {
            var bytes = new List<byte>();
            foreach (var b in Position.GetBytes())
                bytes.Add(b);
            foreach (var b in Rotation.ToEulerAngles().GetBytes())
                bytes.Add(b);
            foreach (var b in Scale.GetBytes())
                bytes.Add(b);

            _instancedData = bytes.ToArray();
        }

        private void RecalculateMatrix()
        {
            var mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(Scale);
            mat *= Matrix4x4.CreateFromQuaternion(Rotation);
            mat *= Matrix4x4.CreateTranslation(Position);
            _matrix = mat;
        }
    }
}