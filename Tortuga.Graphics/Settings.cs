namespace Tortuga.Settings
{
    /// <summary>
    /// Graphics api level
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
        public static GraphicsApiDebugLevelType GraphicsApiDebugLevel = GraphicsApiDebugLevelType.ErrorAndWarnings;
    }
}