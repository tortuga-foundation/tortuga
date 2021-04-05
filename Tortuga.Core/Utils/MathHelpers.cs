using System;

namespace Tortuga.Core
{
    /// <summary>
    /// Additional helper functions for math library
    /// </summary>
    public static partial class MathHelpers
    {
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