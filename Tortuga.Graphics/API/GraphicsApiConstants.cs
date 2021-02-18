#pragma warning disable CS1591
using System.Collections.Generic;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public static partial class GraphicsApiConstants
    {
        public static FixedUtf8String VK_KHR_SURFACE_EXTENSION_NAME = "VK_KHR_surface";
        public static FixedUtf8String VK_KHR_WIN32_SURFACE_EXTENSION_NAME = "VK_KHR_win32_surface";
        public static FixedUtf8String VK_KHR_XCB_SURFACE_EXTENSION_NAME = "VK_KHR_xcb_surface";
        public static FixedUtf8String VK_KHR_XLIB_SURFACE_EXTENSION_NAME = "VK_KHR_xlib_surface";
        public static FixedUtf8String VK_MVK_SURFACE_EXTENSION_NAME = "VK_MVK_macos_surface";
        public static FixedUtf8String VK_KHR_SWAPCHAIN_EXTENSION_NAME = "VK_KHR_swapchain";
        public static FixedUtf8String VK_EXT_DEBUG_REPORT_EXTENSION_NAME = "VK_EXT_debug_report";
        public static FixedUtf8String MAIN = "main";

        public static List<VkFormat> DEPTH_FORMAT_CANDIDATES = new List<VkFormat>()
        {
            VkFormat.D32Sfloat,
            VkFormat.D32SfloatS8Uint,
            VkFormat.D24UnormS8Uint
        };
    }
}