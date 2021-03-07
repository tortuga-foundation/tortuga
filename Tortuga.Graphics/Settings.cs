using System;

namespace Tortuga.Graphics
{
    /// <summary>
    /// different types of debug level
    /// </summary>
    [Flags]
    public enum GraphicsApiDebugLevelType
    {
        /// <summary>
        /// disable graphics api output
        /// This is for production use
        /// </summary> 
        None,
        /// <summary>
        /// enables errors
        /// </summary>
        Error,
        /// <summary>
        /// enables warnings
        /// </summary>
        Warning,
        /// <summary>
        /// enables information
        /// </summary>
        Info,
        /// <summary>
        /// enables verbose input
        /// </summary>
        Debug
    }

    /// <summary>
    /// Tortuga engine settings
    /// </summary>
    public static partial class Settings
    {
        /// <summary>
        /// what type of debug information should the graphics api output
        /// </summary>
        public static GraphicsApiDebugLevelType GraphicsApiDebugLevel = (
            GraphicsApiDebugLevelType.None
        );

        internal static bool IsGraphicsApiDebugingEnabled
        {
            get
            {
                return (
                    (Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Error) != 0 ||
                    (Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Warning) != 0 ||
                    (Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Info) != 0 ||
                    (Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Debug) != 0
                );
            }
        }
    }
}