using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace Tortuga
{
    /// <summary>
    /// Uses bit converter class to extend GetBytes methods
    /// </summary>
    public static class BitConverterExtension
    {
        /// <summary>
        /// convert's int to byte array
        /// </summary>
        public static byte[] GetBytes(this int obj)
            => BitConverter.GetBytes(obj);
        /// <summary>
        /// converts uint to byte array
        /// </summary>
        public static byte[] GetBytes(this uint obj)
            => BitConverter.GetBytes(obj);
        /// <summary>
        /// converts int array to byte array
        /// </summary>
        public static byte[] GetBytes(this int[] obj)
        {
            var bytes = new List<byte>();
            foreach (var i in obj)
                bytes.AddRange(BitConverter.GetBytes(i));
            return bytes.ToArray();
        }
        /// <summary>
        /// converts float to byte array
        /// </summary>
        public static byte[] GetBytes(this float obj)
            => BitConverter.GetBytes(obj);
        /// <summary>
        /// converts float array to byte array
        /// </summary>
        public static byte[] GetBytes(this float[] obj)
        {
            var bytes = new List<byte>();
            foreach (var i in obj)
                foreach (var b in BitConverter.GetBytes(i))
                    bytes.Add(b);
            return bytes.ToArray();
        }
        /// <summary>
        /// converts 4x4 matrix to byte array
        /// </summary>
        public static byte[] GetBytes(this Matrix4x4 obj)
        {
            var bytes = new List<byte>();
            //1
            bytes.AddRange(BitConverter.GetBytes(obj.M11));
            bytes.AddRange(BitConverter.GetBytes(obj.M12));
            bytes.AddRange(BitConverter.GetBytes(obj.M13));
            bytes.AddRange(BitConverter.GetBytes(obj.M14));
            //2
            bytes.AddRange(BitConverter.GetBytes(obj.M21));
            bytes.AddRange(BitConverter.GetBytes(obj.M22));
            bytes.AddRange(BitConverter.GetBytes(obj.M23));
            bytes.AddRange(BitConverter.GetBytes(obj.M24));
            //3
            bytes.AddRange(BitConverter.GetBytes(obj.M31));
            bytes.AddRange(BitConverter.GetBytes(obj.M32));
            bytes.AddRange(BitConverter.GetBytes(obj.M33));
            bytes.AddRange(BitConverter.GetBytes(obj.M34));
            //4
            bytes.AddRange(BitConverter.GetBytes(obj.M41));
            bytes.AddRange(BitConverter.GetBytes(obj.M42));
            bytes.AddRange(BitConverter.GetBytes(obj.M43));
            bytes.AddRange(BitConverter.GetBytes(obj.M44));

            return bytes.ToArray();
        }
        /// <summary>
        /// converts vector 2 to byte array
        /// </summary>
        public static byte[] GetBytes(this Vector2 obj)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(obj.X));
            bytes.AddRange(BitConverter.GetBytes(obj.Y));
            return bytes.ToArray();
        }
        /// <summary>
        /// converts vector 3 to byte array
        /// </summary>
        public static byte[] GetBytes(this Vector3 obj)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(obj.X));
            bytes.AddRange(BitConverter.GetBytes(obj.Y));
            bytes.AddRange(BitConverter.GetBytes(obj.Z));
            return bytes.ToArray();
        }
        /// <summary>
        /// converts vector 4 to byte array
        /// </summary>
        public static byte[] GetBytes(this Vector4 obj)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(obj.X));
            bytes.AddRange(BitConverter.GetBytes(obj.Y));
            bytes.AddRange(BitConverter.GetBytes(obj.Z));
            bytes.AddRange(BitConverter.GetBytes(obj.W));
            return bytes.ToArray();
        }
    }
}