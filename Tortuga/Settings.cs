
namespace Tortuga
{
    /// <summary>
    /// Main Settings that is used by the game engine
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Vulkan specific settings used by the game engine
        /// </summary>
        public static class Vulkan
        {
            /// <summary>
            /// Different debug types for vulkan
            /// </summary>
            public enum DebugType
            {
                /// <summary>
                /// No vulkan debuging
                /// </summary>
                None,
                /// <summary>
                /// Vulkan should show error and warnings
                /// </summary>
                ErrorAndWarnings,
                /// <summary>
                /// Vulkan should show full debug info
                /// </summary>
                Full
            };
            /// <summary>
            /// Set Front face for vulkan back face culling
            /// </summary>
            public enum FrontFaceType
            {
                /// <summary>
                /// Don't Cull back faces
                /// </summary>
                None,
                /// <summary>
                /// Cull Counter clock wise faces
                /// </summary>
                Clockwise,
                /// <summary>
                /// Cull clockwise faces
                /// </summary>
                CounterClockwise
            };

            /// <summary>
            /// Set the debug level for vulkan
            /// </summary>
            public static DebugType DebugLevel = DebugType.ErrorAndWarnings;
            /// <summary>
            /// Set front face for vulkan pipeline
            /// WARNING: adjusting this might break UI elements
            /// </summary>
            public static FrontFaceType FrontFace = FrontFaceType.CounterClockwise;
        }
        /// <summary>
        /// Engine window settings
        /// </summary>
        public static class Window
        {
            /// <summary>
            /// Window Types
            /// </summary>
            public enum WindowType
            {
                /// <summary>
                /// Window must be fullscreen only
                /// </summary>
                Fullscreen,
                /// <summary>
                /// Window must be borderless
                /// </summary>
                Borderless,
                /// <summary>
                /// Window must be a non-resizable window
                /// </summary>
                Windowed,
                /// <summary>
                /// Window must be a resizeable window
                /// </summary>
                ResizeableWindow
            }

            /// <summary>
            /// Set the current type of window
            /// </summary>
            public static WindowType Type = WindowType.ResizeableWindow;
        }

        /// <summary>
        /// Graphics settings for the engine
        /// </summary>
        public static class Graphics
        {
            /// <summary>
            /// Controls the resolution the cameras will render at multiplied by the window resolution
            /// </summary>
            public static float RenderResolutionScale = 1.0f;
        }
    }
}