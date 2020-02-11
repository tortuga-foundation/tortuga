
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

            public static DebugType DebugLevel = DebugType.Full;
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