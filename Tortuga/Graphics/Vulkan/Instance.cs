using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics
{
    public class VulkanInstance
    {
        public VkInstance Instance => _instance;
        private PFN_vkDebugReportCallbackEXT _debugCallbackFunc;
        VkDebugReportCallbackEXT _debugReportCallbackHandle;
        private VkInstance _instance;
        private List<Device> _devices = new List<Device>();

        //debug callback report callback function
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
            string message = Util.GetString(pMessage);
            var debugReportFlags = (VkDebugReportFlagsEXT)flags;
            string fullMessage = $"[{debugReportFlags}] ({objectType}) {message}";
            Console.WriteLine(fullMessage);
            return VkBool32.True;
        }

        //debug callback report delegates
        private unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
            VkInstance instance,
            VkDebugReportCallbackCreateInfoEXT* createInfo,
            IntPtr allocatorPtr,
            out VkDebugReportCallbackEXT ret);

        private unsafe delegate void vkDestroyDebugReportCallbackEXT_d(
            VkInstance instance,
            VkDebugReportCallbackEXT callback,
            VkAllocationCallbacks* pAllocator);

        //debug callback report create & destroy
        private unsafe void CreateVulkanDebugReportCallback()
        {
            //setup debug report callback create info
            _debugCallbackFunc = DebugCallback;
            var debugCallbackReportInfo = VkDebugReportCallbackCreateInfoEXT.New();
            debugCallbackReportInfo.flags = Settings.Vulkan.DebugFlags;
            debugCallbackReportInfo.pfnCallback = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            VkDebugReportCallbackEXT debugReportCallback;

            //find create debug report callback function
            IntPtr debugCreateFnPtr;
            using (FixedUtf8String debugExtFnName = Strings.VK_CREATE_DEBUG_REPORT_CALLBACK_FUNC)
                debugCreateFnPtr = vkGetInstanceProcAddr(this._instance, debugExtFnName);
            if (debugCreateFnPtr == IntPtr.Zero)
                throw new Exception("could not load vulkan report debug callback function");

            //create debug report callback
            var createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(debugCreateFnPtr);
            if (createDelegate(this._instance, &debugCallbackReportInfo, IntPtr.Zero, out debugReportCallback) != VkResult.Success)
                throw new Exception("failed to create debug callback report");
            this._debugReportCallbackHandle = debugReportCallback;
        }
        private unsafe void DestroyVulkanDebugReportCallback()
        {
            var debugExtFnName = Strings.VK_DESTROY_DEBUG_REPORT_CALLBACK_FUNC;
            var destroyFuncPtr = vkGetInstanceProcAddr(this._instance, debugExtFnName);
            var destroyDel = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugReportCallbackEXT_d>(destroyFuncPtr);
            destroyDel(this._instance, this._debugReportCallbackHandle, null);
        }

        //vulkan instance constructor
        public unsafe VulkanInstance()
        {
            //vulkan extensions
            var enabledExtensions = new NativeList<IntPtr>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                enabledExtensions.Add(Strings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                enabledExtensions.Add(Strings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            else
                throw new PlatformNotSupportedException();
            enabledExtensions.Add(Strings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
            enabledExtensions.Add(Strings.VK_KHR_SURFACE_EXTENSION_NAME);

            //vulkan validation layers
            var enabledLayers = new NativeList<IntPtr>();
            if (Settings.Vulkan.EnableDebugValidation)
                enabledLayers.Add(Strings.StandardValidationLayeName);

            //create vulkan instance
            var instanceInfo = VkInstanceCreateInfo.New();
            instanceInfo.enabledExtensionCount = enabledExtensions.Count;
            instanceInfo.ppEnabledExtensionNames = (byte**)enabledExtensions.Data;
            instanceInfo.enabledLayerCount = enabledLayers.Count;
            instanceInfo.ppEnabledLayerNames = (byte**)enabledLayers.Data;
            VkInstance instance;
            if (vkCreateInstance(&instanceInfo, null, &instance) != VkResult.Success)
                throw new NotSupportedException("Could not create vulkan instance");
            this._instance = instance;

            //vulkan callbacks
            if (Settings.Vulkan.EnableDebugValidation)
                this.CreateVulkanDebugReportCallback();

            //get physical devies
            uint physicalDeviceCount = 0;
            if (vkEnumeratePhysicalDevices(instance, &physicalDeviceCount, null) != VkResult.Success)
                throw new PlatformNotSupportedException("Could not ask vulkan for a list of devices");
            IntPtr* physicalDevices = stackalloc IntPtr[(int)physicalDeviceCount];
            if (vkEnumeratePhysicalDevices(instance, &physicalDeviceCount, (VkPhysicalDevice*)physicalDevices) != VkResult.Success)
                throw new PlatformNotSupportedException("Could not ask vulkan for a list of devices");

            //initialize devices
            for (int i = 0; i < physicalDeviceCount; i++)
                _devices.Add(new Device(physicalDevices[i]));
        }
        unsafe ~VulkanInstance()
        {
            if (Settings.Vulkan.EnableDebugValidation)
                this.DestroyVulkanDebugReportCallback();
            vkDestroyInstance(_instance, null);
        }
    }
}