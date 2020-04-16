using System;
using System.Runtime.InteropServices;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class VulkanInstance
    {
        public VkInstance Handle => _instanceHandle;
        public Device[] Devices => _devices;
        
        private VkInstance _instanceHandle;
        private VkDebugReportCallbackEXT _debugReportCallbackHandle;
        private PFN_vkDebugReportCallbackEXT _debugCallbackFunc;

        private Device[] _devices;

        private struct VkVersion
        {
            private readonly uint value;

            public VkVersion(uint major, uint minor, uint patch)
            {
                value = major << 22 | minor << 12 | patch;
            }

            public uint Major => value >> 22;

            public uint Minor => (value >> 12) & 0x3ff;

            public uint Patch => (value >> 22) & 0xfff;

            public static implicit operator uint(VkVersion version)
            {
                return version.value;
            }
        }

        public unsafe VulkanInstance()
        {
            //vulkan extensions
            var instanceExtensions = new NativeList<IntPtr>();
            instanceExtensions.Add(Strings.VK_KHR_SURFACE_EXTENSION_NAME);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                instanceExtensions.Add(Strings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                instanceExtensions.Add(Strings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            else
                throw new PlatformNotSupportedException("this platform is not supported");

            if (Settings.Vulkan.DebugLevel != Settings.Vulkan.DebugType.None)
                instanceExtensions.Add(Strings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);


            //vulkan validation layers
            var validationLayer = new NativeList<IntPtr>();
            if (Settings.Vulkan.DebugLevel != Settings.Vulkan.DebugType.None)
                validationLayer.Add(Strings.StandardValidationLayeName);

            //create vulkan info
            var applicationInfo = VkApplicationInfo.New();
            applicationInfo.apiVersion = new VkVersion(1, 0, 0);
            applicationInfo.applicationVersion = new VkVersion(1, 0, 0);
            applicationInfo.engineVersion = new VkVersion(1, 0, 0);
            applicationInfo.pApplicationName = new FixedUtf8String("Tortuga");
            applicationInfo.pEngineName = new FixedUtf8String("Tortuga");

            var instanceInfo = VkInstanceCreateInfo.New();
            instanceInfo.pApplicationInfo = &applicationInfo;
            instanceInfo.enabledExtensionCount = instanceExtensions.Count;
            instanceInfo.ppEnabledExtensionNames = (byte**)instanceExtensions.Data;
            instanceInfo.enabledLayerCount = validationLayer.Count;
            instanceInfo.ppEnabledLayerNames = (byte**)validationLayer.Data;

            //create vulkan instance
            VkInstance instance;
            if (vkCreateInstance(&instanceInfo, null, &instance) != VkResult.Success)
                throw new Exception("failed to initialize vulkan");
            this._instanceHandle = instance;

            //setup debugging callbacks
            if (Settings.Vulkan.DebugLevel != Settings.Vulkan.DebugType.None)
            {
                var flags = VkDebugReportFlagsEXT.None;
                if (Settings.Vulkan.DebugLevel == Settings.Vulkan.DebugType.ErrorAndWarnings)
                    flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT;
                else if (Settings.Vulkan.DebugLevel == Settings.Vulkan.DebugType.Full)
                    flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT |
                    VkDebugReportFlagsEXT.DebugEXT | VkDebugReportFlagsEXT.PerformanceWarningEXT | VkDebugReportFlagsEXT.InformationEXT;
                if (CreateDebugReportCallback(flags) != VkResult.Success)
                    Console.WriteLine("could not enable vulkan validation layer");
            }

            uint deviceCount = 0;
            if (vkEnumeratePhysicalDevices(this._instanceHandle, ref deviceCount, null) != VkResult.Success)
                throw new Exception("could not get physical devices");
            var physicalDevices = new NativeList<VkPhysicalDevice>(deviceCount);
            physicalDevices.Count = deviceCount;
            if (vkEnumeratePhysicalDevices(this._instanceHandle, ref deviceCount, (VkPhysicalDevice*)physicalDevices.Data.ToPointer()) != VkResult.Success)
                throw new Exception("could not get physical devices");

            this._devices = new Device[deviceCount];
            for (int i = 0; i < deviceCount; i++)
                this._devices[i] = new Device(physicalDevices[i]);
        }
        unsafe ~VulkanInstance()
        {
            if (Settings.Vulkan.DebugLevel != Settings.Vulkan.DebugType.None)
                DestroyDebugReportCallback();
            vkDestroyInstance(_instanceHandle, null);
        }

        private unsafe uint DebugCallback(
            uint flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong @object,
            UIntPtr location,
            int messageCode,
            byte* pLayerPrefix,
            byte* pMessage,
            void* pUserData)
        {
            Console.WriteLine(Util.GetString(pMessage));
            return 0;
        }

        internal unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackCreateInfoEXT* createInfo,
        IntPtr allocatorPtr,
        out VkDebugReportCallbackEXT ret);

        internal unsafe delegate void vkDestroyDebugReportCallbackEXT_d(
            VkInstance instance,
            VkDebugReportCallbackEXT callback,
            VkAllocationCallbacks* pAllocator);

        private unsafe VkResult CreateDebugReportCallback(VkDebugReportFlagsEXT flags)
        {
            _debugCallbackFunc = DebugCallback;
            var debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            var debugCallbackInfo = VkDebugReportCallbackCreateInfoEXT.New();
            debugCallbackInfo.flags = flags;
            debugCallbackInfo.pfnCallback = debugFunctionPtr;

            var createFnPtr = vkGetInstanceProcAddr(this._instanceHandle, new FixedUtf8String("vkCreateDebugReportCallbackEXT"));
            if (createFnPtr == IntPtr.Zero)
                return VkResult.ErrorValidationFailedEXT;

            var createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(createFnPtr);
            return createDelegate(this._instanceHandle, &debugCallbackInfo, IntPtr.Zero, out _debugReportCallbackHandle);
        }

        private unsafe void DestroyDebugReportCallback()
        {
            _debugCallbackFunc = null;
            var destroyFuncPtr = vkGetInstanceProcAddr(_instanceHandle, new FixedUtf8String("vkDestroyDebugReportCallbackEXT"));
            var destroyDel = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugReportCallbackEXT_d>(destroyFuncPtr);
            destroyDel(this._instanceHandle, _debugReportCallbackHandle, null);
        }
    }
}