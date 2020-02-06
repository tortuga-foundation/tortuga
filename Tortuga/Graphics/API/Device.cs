using System;
using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;


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
        public QueueFamily GraphicsQueueFamily
        {
            get
            {
                foreach (var q in _queueFamilyProperties)
                    if (q.IsGraphics)
                        return q;

                throw new Exception("failed to find graphics queue in this device");
            }
        }

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

        unsafe ~Device()
        {
            vkDestroyDevice(_logicalDevice, null);
        }

        public void WaitForQueue(VkQueue queue)
        {
            if (vkQueueWaitIdle(queue) != VkResult.Success)
                throw new Exception("failed to wait on device queue");
        }

        public unsafe VkFormat FindDepthFormat
        {
            get
            {
                var candidates = new List<VkFormat>(){
                    VkFormat.D32Sfloat, VkFormat.D32SfloatS8Uint, VkFormat.D24UnormS8Uint
                };
                var tiling = VkImageTiling.Optimal;
                var features = VkFormatFeatureFlags.DepthStencilAttachment;

                foreach (var format in candidates)
                {
                    VkFormatProperties formatProperties;
                    vkGetPhysicalDeviceFormatProperties(_physicalDevice, format, &formatProperties);

                    if (tiling == VkImageTiling.Linear && (formatProperties.linearTilingFeatures & features) == features)
                        return format;
                    else if (tiling == VkImageTiling.Optimal && (formatProperties.optimalTilingFeatures & features) == features)
                        return format;
                }
                throw new Exception("failed to find any depth format supported by this device");
            }
        }

        public uint FindMemoryType(uint typeFilter, VkMemoryPropertyFlags properties)
        {
            for (uint i = 0; i < _memoryProperties.memoryTypeCount; i++)
            {
                if ((typeFilter & (i >> 1)) != 0)
                {
                    if ((GetMemoryType(i).propertyFlags & properties) == properties)
                        return i;
                }
            }

            throw new Exception("failed to find suitable memory type on device");
        }

        private VkMemoryType GetMemoryType(uint i)
        {
            switch (i)
            {
                case 0:
                    return _memoryProperties.memoryTypes_0;
                case 1:
                    return _memoryProperties.memoryTypes_1;
                case 2:
                    return _memoryProperties.memoryTypes_2;
                case 3:
                    return _memoryProperties.memoryTypes_3;
                case 4:
                    return _memoryProperties.memoryTypes_4;
                case 5:
                    return _memoryProperties.memoryTypes_5;
                case 6:
                    return _memoryProperties.memoryTypes_6;
                case 7:
                    return _memoryProperties.memoryTypes_7;
                case 8:
                    return _memoryProperties.memoryTypes_8;
                case 9:
                    return _memoryProperties.memoryTypes_9;
                case 10:
                    return _memoryProperties.memoryTypes_10;
                case 11:
                    return _memoryProperties.memoryTypes_11;
                case 12:
                    return _memoryProperties.memoryTypes_12;
                case 13:
                    return _memoryProperties.memoryTypes_13;
                case 14:
                    return _memoryProperties.memoryTypes_14;
                case 15:
                    return _memoryProperties.memoryTypes_15;
                case 16:
                    return _memoryProperties.memoryTypes_16;
                case 17:
                    return _memoryProperties.memoryTypes_17;
                case 18:
                    return _memoryProperties.memoryTypes_18;
                case 19:
                    return _memoryProperties.memoryTypes_19;
                case 20:
                    return _memoryProperties.memoryTypes_20;
                case 21:
                    return _memoryProperties.memoryTypes_21;
                case 22:
                    return _memoryProperties.memoryTypes_22;
                case 23:
                    return _memoryProperties.memoryTypes_23;
                case 24:
                    return _memoryProperties.memoryTypes_24;
                case 25:
                    return _memoryProperties.memoryTypes_25;
                case 26:
                    return _memoryProperties.memoryTypes_26;
                case 27:
                    return _memoryProperties.memoryTypes_27;
                case 28:
                    return _memoryProperties.memoryTypes_28;
                case 29:
                    return _memoryProperties.memoryTypes_29;
                case 30:
                    return _memoryProperties.memoryTypes_30;
                case 31:
                    return _memoryProperties.memoryTypes_31;
                default:
                    throw new NotSupportedException("this type of memory is not supported");
            }
        }
    }
}