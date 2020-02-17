
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

            public static DebugType DebugLevel = DebugType.Full;
            public static FrontFaceType FrontFace = FrontFaceType.None;
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
    }
}