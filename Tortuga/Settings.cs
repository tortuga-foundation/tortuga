using Vulkan;

namespace Tortuga
{
    public static class Settings
    {
        public static class Vulkan
        {
            public static VkDebugReportFlagsEXT DebugFlags = VkDebugReportFlagsEXT.InformationEXT | VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.PerformanceWarningEXT | VkDebugReportFlagsEXT.DebugEXT | VkDebugReportFlagsEXT.ErrorEXT;
            public static bool EnableDebugValidation = true;
        }
    }
}