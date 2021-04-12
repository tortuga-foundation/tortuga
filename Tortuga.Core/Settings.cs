namespace Tortuga
{
    /// <summary>
    /// Core tortuga settings
    /// </summary>
    public static partial class Settings
    {
        /// <summary>
        /// How many times does the main loop run, per second,
        /// 0 = No Limit
        /// Note: Can also use this to lock the FPS
        /// </summary>
        public static float MaxLoopsPerSecond = 0.0f;
    }
}