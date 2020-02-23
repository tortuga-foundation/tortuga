
namespace Tortuga
{
    public static class Settings
    {
        public static class Vulkan
        {
            public enum DebugType
            {
                None,
                ErrorAndWarnings,
                Full
            };
            public enum FrontFaceType
            {
                None,
                Clockwise,
                CounterClockwise
            };

            public static DebugType DebugLevel = DebugType.ErrorAndWarnings;
            public static FrontFaceType FrontFace = FrontFaceType.CounterClockwise;
        }
        public static class Window
        {
            public enum WindowType
            {
                Fullscreen,
                Borderless,
                Windowed,
                ResizeableWindow
            }

            public static WindowType Type = WindowType.ResizeableWindow;
        }
        public static class Graphics
        {
            public static float RenderResolutionScale = 1.0f;
            public static uint TextureSize = 1024;
        }
    }
}