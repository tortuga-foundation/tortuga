using System;
using System.Numerics;

namespace Tortuga.Core
{
    /// <summary>
    /// Additional helper functions for math library
    /// </summary>
    public static partial class MathHelpers
    {
        /// <summary>
        /// converts quaternion to euler angles
        /// </summary>
        /// <param name="q">quaternion to convert</param>
        /// <returns>vector 3 of eular angles</returns>
        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            var pitchYawRoll = new Vector3();
            var sqz = q.Z * q.Z;
            var sqw = q.W * q.W;
            var sqy = q.Y * q.Y;
            // yaw 
            pitchYawRoll.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (sqz + sqw));
            // pitch 
            pitchYawRoll.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));
            // roll
            pitchYawRoll.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (sqy + sqz));
            return pitchYawRoll;
        }

        /// <summary>
        /// Converts a float from degrees to radians
        /// </summary>
        public static float ToRadians(this float degree)
        {
            return (degree / 360.0f) * MathF.PI;
        }

        /// <summary>
        /// Converts a float from radians to degrees
        /// </summary>
        public static float ToDegree(this float radians)
        {
            return (radians / MathF.PI) * 360.0f;
        }
    }
}