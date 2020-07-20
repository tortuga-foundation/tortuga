namespace Tortuga.Settings
{
    /// <summary>
    /// Core tortuga settings
    /// </summary>
    public class Core
    {
        /// <summary>
        /// How many times does the main loop run, per second,
        /// 0 = No Limit
        /// Note: Can also use this to lock the FPS
        /// </summary>
        public static float MaxLoopsPerSecond = 60.0f;
    }
}