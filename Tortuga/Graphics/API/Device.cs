using System;
using System.Collections.Generic;
using Veldrid.Sdl2;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Sdl2.Sdl2Native;


namespace Tortuga.Graphics.API
{
    internal class Device
    {
        public struct QueueFamily
        {
            public uint Index;
            public uint Count;
            public bool IsGraphics;
            public bool IsCompute;
            public bool IsTransfer;
            public List<VkQueue> Queues;
        };

        public VkPhysicalDevice PhysicalDevice => _physicalDevice;
        public VkDevice LogicalDevice => _logicalDevice;
        public VkPhysicalDeviceProperties Properties => _properties;
        public VkPhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;
        public VkPhysicalDeviceFeatures Feature => _features;
        public List<QueueFamily> QueueFamilyProperties => _queueFamilyProperties;

        private VkPhysicalDevice _physicalDevice;
        private VkDevice _logicalDevice;
        private VkPhysicalDeviceProperties _properties;
        private VkPhysicalDeviceMemoryProperties _memoryProperties;
        private VkPhysicalDeviceFeatures _features;
        private List<QueueFamily> _queueFamilyProperties;

        public unsafe Device(VkPhysicalDevice physicalDevice)
        {
            this._physicalDevice = physicalDevice;

            //get physical device information
            vkGetPhysicalDeviceProperties(_physicalDevice, out _properties);
            vkGetPhysicalDeviceMemoryProperties(_physicalDevice, out _memoryProperties);
            vkGetPhysicalDeviceFeatures(_physicalDevice, out _features);

            //get family queue properties
            uint familyQueuePropertiesCount;
            vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &familyQueuePropertiesCount, null);
            var familyQueueProperties = new NativeList<VkQueueFamilyProperties>(familyQueuePropertiesCount);
            familyQueueProperties.Count = familyQueuePropertiesCount;
            vkGetPhysicalDeviceQueueFamilyProperties(
                _physicalDevice,
                &familyQueuePropertiesCount,
                (VkQueueFamilyProperties*)familyQueueProperties.Data.ToPointer()
            );

            _queueFamilyProperties = new List<QueueFamily>();
            for (int i = 0; i < familyQueueProperties.Count; i++)
            {
                var familyQueueProperty = familyQueueProperties[i];
                _queueFamilyProperties.Add(new QueueFamily
                {
                    Index = Convert.ToUInt32(i),
                    Count = familyQueueProperty.queueCount,
                    IsGraphics = (familyQueueProperty.queueFlags & VkQueueFlags.Graphics) != 0,
                    IsCompute = (familyQueueProperty.queueFlags & VkQueueFlags.Compute) != 0,
                    IsTransfer = (familyQueueProperty.queueFlags & VkQueueFlags.Transfer) != 0,
                    Queues = new List<VkQueue>() //these will get populated after device is initialized
                });
            }

            var queueCreateInfoCount = Convert.ToUInt32(_queueFamilyProperties.Count);
            var queueCreateInfo = new NativeList<VkDeviceQueueCreateInfo>(queueCreateInfoCount);
            queueCreateInfo.Count = queueCreateInfoCount;
            var priorities = new NativeList<float>[_queueFamilyProperties.Count];
            for (int i = 0; i < queueCreateInfoCount; i++)
            {
                //initialize priorities
                priorities[i] = new NativeList<float>(_queueFamilyProperties[i].Count);
                priorities[i].Count = _queueFamilyProperties[i].Count;
                for (int j = 0; j < _queueFamilyProperties[i].Count; j++)
                    priorities[i][j] = 1.0f / _queueFamilyProperties[i].Count;

                //setup queue create info
                queueCreateInfo[i] = VkDeviceQueueCreateInfo.New();
                queueCreateInfo[i].queueCount = _queueFamilyProperties[i].Count;
                queueCreateInfo[i].queueFamilyIndex = _queueFamilyProperties[i].Index;
                queueCreateInfo[i].pQueuePriorities = (float*)priorities[i].Data.ToPointer();
            }

            var enabledFeatures = new VkPhysicalDeviceFeatures();
            enabledFeatures.samplerAnisotropy = true;

            var enabledExtensions = new NativeList<IntPtr>();
            enabledExtensions.Add(Strings.VK_KHR_SWAPCHAIN_EXTENSION_NAME);

            var deviceInfo = VkDeviceCreateInfo.New();
            deviceInfo.pEnabledFeatures = &enabledFeatures;
            deviceInfo.enabledExtensionCount = enabledExtensions.Count;
            deviceInfo.ppEnabledExtensionNames = (byte**)enabledExtensions.Data;
            deviceInfo.enabledLayerCount = 0;
            deviceInfo.ppEnabledLayerNames = null;
            deviceInfo.queueCreateInfoCount = queueCreateInfo.Count;
            deviceInfo.pQueueCreateInfos = (VkDeviceQueueCreateInfo*)queueCreateInfo.Data.ToPointer();

            VkDevice device;
            if (vkCreateDevice(_physicalDevice, &deviceInfo, null, &device) != VkResult.Success)
                throw new Exception("failed to initialize device");
            _logicalDevice = device;

            //get queues
            for (int i = 0; i < _queueFamilyProperties.Count; i++)
            {
                for (uint j = 0; j < _queueFamilyProperties[i].Count; j++)
                {
                    VkQueue queue;
                    vkGetDeviceQueue(_logicalDevice, _queueFamilyProperties[i].Index, j, &queue);
                    _queueFamilyProperties[i].Queues.Add(queue);
                }
            }
        }
    }
}