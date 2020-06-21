using Tortuga.Graphics;

namespace Tortuga.Settings
{
    /// <summary>
    /// Graphics api debug level
    /// </summary>
    public enum GraphicsApiDebugLevelType
    {
        /// <summary>
        /// No Graphics API Debuging
        /// </summary>
        None,
        /// <summary>
        /// Show Graphics API Errors and Warnings
        /// </summary>
        ErrorAndWarnings,
        /// <summary>
        /// Show full Graphics API Information
        /// </summary>
        Full
    }

    /// <summary>
    /// Graphics Settings
    /// </summary>
    public static class Graphics
    {
        /// <summary>
        /// Graphics api debug level
        /// Changing this will require restart and this should be setup before the engine is loaded
        /// </summary>
        public static GraphicsApiDebugLevelType GraphicsApiDebugLevel = GraphicsApiDebugLevelType.Full;
        /// <summary>
        /// How window is displayed
        /// </summary>
        public static WindowType Window = WindowType.Resizeable;
    }
}