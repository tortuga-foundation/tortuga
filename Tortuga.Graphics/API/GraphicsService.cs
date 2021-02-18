#pragma warning disable CS1591
using System;
using Vulkan;
using Tortuga.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Tortuga.Graphics.API
{
    public class GraphicsService
    {
        public Device PrimaryDevice => _devices[0];
        public List<Device> Devices => _devices;
        public VkInstance Handle => _handle;

        private VkInstance _handle;
        private List<Device> _devices;
        private VkDebugReportCallbackEXT _debugCallbackHandle;
        private PFN_vkDebugReportCallbackEXT _debugCallbackFunction;

        public unsafe GraphicsService()
        {
            //create vulkan info
            var applicationInfo = new VkApplicationInfo
            {
                sType = VkStructureType.ApplicationInfo,
                apiVersion = new VkVersion(1, 0, 0),
                applicationVersion = new VkVersion(1, 0, 0),
                engineVersion = new VkVersion(1, 0, 0),
                pApplicationName = new FixedUtf8String("Tortuga"),
                pEngineName = new FixedUtf8String("Tortuga")
            };

            //vulkan extensions
            var instanceExtensions = new NativeList<IntPtr>();
            instanceExtensions.Add(GraphicsApiConstants.VK_KHR_SURFACE_EXTENSION_NAME);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                instanceExtensions.Add(GraphicsApiConstants.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                instanceExtensions.Add(GraphicsApiConstants.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                instanceExtensions.Add(GraphicsApiConstants.VK_MVK_SURFACE_EXTENSION_NAME);
            else
                throw new NotSupportedException("This platform is not supported");

            if (Settings.EnableGraphicsApiDebugging)
                instanceExtensions.Add(GraphicsApiConstants.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);

            //vulkan validation layers
            var validationLayer = new NativeList<IntPtr>();
            if (Settings.EnableGraphicsApiDebugging)
            {
                uint supportedLayersCount;
                VulkanNative.vkEnumerateInstanceLayerProperties(&supportedLayersCount, null);
                var supportedLayers = new NativeList<VkLayerProperties>(supportedLayersCount);
                supportedLayers.Count = supportedLayersCount;
                VulkanNative.vkEnumerateInstanceLayerProperties(
                    &supportedLayersCount,
                    (VkLayerProperties*)supportedLayers.Data.ToPointer()
                );
                foreach (var vl in supportedLayers)
                    validationLayer.Add(new IntPtr(vl.layerName));
            }

            var instanceInfo = new VkInstanceCreateInfo
            {
                sType = VkStructureType.InstanceCreateInfo,
                pApplicationInfo = &applicationInfo,
                enabledExtensionCount = instanceExtensions.Count,
                ppEnabledExtensionNames = (byte**)instanceExtensions.Data,
                enabledLayerCount = validationLayer.Count,
                ppEnabledLayerNames = (byte**)validationLayer.Data
            };

            VkInstance instance;
            if (VulkanNative.vkCreateInstance(
                &instanceInfo,
                null,
                &instance
            ) != VkResult.Success)
                throw new Exception("failed to initialize graphics api");
            _handle = instance;

            if (Settings.EnableGraphicsApiDebugging)
            {
                if (CreateDebugReportCallback(
                    GetVulkanDebugFlags
                ) != VkResult.Success)
                    throw new Exception("failed to start graphics api debugger");
            }
            //get vulkan physical device list
            uint deviceCount = 0;
            if (VulkanNative.vkEnumeratePhysicalDevices(
                _handle,
                ref deviceCount,
                null
            ) != VkResult.Success)
                throw new Exception("could not get physical devices");
            var physicalDevices = new NativeList<VkPhysicalDevice>(deviceCount);
            physicalDevices.Count = deviceCount;
            if (VulkanNative.vkEnumeratePhysicalDevices(
                _handle,
                ref deviceCount,
                (VkPhysicalDevice*)physicalDevices.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("could not get physical devices");

            //setup devices
            _devices = new List<Device>();
            for (int i = 0; i < deviceCount; i++)
                _devices.Add(new Device(physicalDevices[i]));

            //sort devices with best to worst
            _devices.Sort((a, b) => b.Score.CompareTo(a.Score));
        }

        unsafe ~GraphicsService()
        {
            if (Settings.EnableGraphicsApiDebugging)
                DestroyDebugReportCallback();

            if (_handle != VkInstance.Null)
            {
                VulkanNative.vkDestroyInstance(
                    _handle,
                    null
                );
                _handle = VkInstance.Null;
            }
        }

        #region Graphics API Debugger

        private VkDebugReportFlagsEXT GetVulkanDebugFlags
        {
            get
            {
                var debugFlags = VkDebugReportFlagsEXT.None;
                if ((Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Error) != 0)
                    debugFlags |= VkDebugReportFlagsEXT.ErrorEXT;
                if ((Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Warning) != 0)
                    debugFlags |= VkDebugReportFlagsEXT.WarningEXT;
                if ((Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Info) != 0)
                    debugFlags |= VkDebugReportFlagsEXT.InformationEXT | VkDebugReportFlagsEXT.PerformanceWarningEXT;
                if ((Settings.GraphicsApiDebugLevel & GraphicsApiDebugLevelType.Debug) != 0)
                    debugFlags |= VkDebugReportFlagsEXT.DebugEXT;
                return debugFlags;
            }
        }

        private unsafe uint DebugCallback(
            uint flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong @object,
            UIntPtr location,
            int messageCode,
            byte* pLayerPrefix,
            byte* pMessage,
            void* pUserData
        )
        {
            int characters = 0;
            while (pMessage[characters] != 0)
                characters++;

            Console.WriteLine(
                Encoding.UTF8.GetString(
                    pMessage, characters
                )
            );
            return 0;
        }

        private unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
            VkInstance instance,
            VkDebugReportCallbackCreateInfoEXT* createInfo,
            IntPtr allocatorPtr,
            out VkDebugReportCallbackEXT handle
        );

        private unsafe delegate void vkDestroyDebugReportCallbackEXT_d(
            VkInstance instance,
            VkDebugReportCallbackEXT handle,
            VkAllocationCallbacks* pAllocator
        );

        private unsafe VkResult CreateDebugReportCallback(
            VkDebugReportFlagsEXT flags
        )
        {
            _debugCallbackFunction = DebugCallback;
            var debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunction);
            var debugCallbackInfo = new VkDebugReportCallbackCreateInfoEXT
            {
                sType = VkStructureType.DebugReportCallbackCreateInfoEXT,
                flags = flags,
                pfnCallback = debugFunctionPtr
            };

            var createFunctionPtr = VulkanNative.vkGetInstanceProcAddr(
                _handle,
                new FixedUtf8String("vkCreateDebugReportCallbackEXT")
            );
            if (createFunctionPtr == IntPtr.Zero)
                return VkResult.ErrorValidationFailedEXT;

            var createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(
                createFunctionPtr
            );
            return createDelegate(
                _handle,
                &debugCallbackInfo,
                IntPtr.Zero,
                out _debugCallbackHandle
            );
        }

        private unsafe void DestroyDebugReportCallback()
        {
            _debugCallbackFunction = null;
            if (_debugCallbackHandle != VkDebugReportCallbackEXT.Null)
            {
                var destroyFunctionPtr = VulkanNative.vkGetInstanceProcAddr(
                    _handle,
                    new FixedUtf8String("vkDestroyDebugReportCallbackEXT")
                );
                var destroyDelegate = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugReportCallbackEXT_d>(
                    destroyFunctionPtr
                );
                destroyDelegate(
                    _handle,
                    _debugCallbackHandle,
                    null
                );
                _debugCallbackHandle = VkDebugReportCallbackEXT.Null;
            }
        }

        #endregion
    }
}