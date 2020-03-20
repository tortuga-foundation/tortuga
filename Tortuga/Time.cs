using System.Diagnostics;

namespace Tortuga
{
    /// <summary>
    /// Time System for the engine
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Stop watch used to monitor each frame's duration
        /// </summary>
        public static Stopwatch StopWatch;
        /// <summary>
        /// How long it took to render 1 frame
        /// </summary>
        public static float DeltaTime;
        /// <summary>
        /// How long it to render last frame
        /// </summary>
        public static long LastFramesTicks;
    }
}