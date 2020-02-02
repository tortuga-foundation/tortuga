
namespace Tortuga
{
    public static class Settings
    {
        public static class Vulkan
        {
            public enum DebugType
            {
                None,
                Info,
                Debug
            };

            public static DebugType DebugLevel = DebugType.Info;
        }
    }
}