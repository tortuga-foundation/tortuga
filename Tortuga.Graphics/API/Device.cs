#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Device
    {
        public QueueFamily GraphicsQueueFamily
        => _queueFamilies.Find(q => (q.Type & QueueFamilyType.Graphics) != 0);
        public QueueFamily TransferQueueFamily
        => _queueFamilies.Find(q => (q.Type & QueueFamilyType.Transfer) != 0);
        public QueueFamily ComputeQueueFamily
        => _queueFamilies.Find(q => (q.Type & QueueFamilyType.Transfer) != 0);

        public VkDevice Handle => _handle;
        public VkPhysicalDevice PhysicalDevice => _physicalDevice;
        public VkPhysicalDeviceProperties Properties => _properties;
        public VkPhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;
        public VkPhysicalDeviceFeatures Features => _features;
        public List<QueueFamily> QueueFamilies => _queueFamilies;
        public float Score => _score;

        private VkPhysicalDevice _physicalDevice;
        private VkPhysicalDeviceProperties _properties;
        private VkPhysicalDeviceMemoryProperties _memoryProperties;
        private VkPhysicalDeviceFeatures _features;
        private List<QueueFamily> _queueFamilies;
        private VkDevice _handle;
        private float _score;

        public unsafe Device(VkPhysicalDevice physicalDevice)
        {
            _physicalDevice = physicalDevice;

            //get physical device information
            VulkanNative.vkGetPhysicalDeviceProperties(
                _physicalDevice,
                out _properties
            );
            VulkanNative.vkGetPhysicalDeviceMemoryProperties(
                _physicalDevice,
                out _memoryProperties
            );
            VulkanNative.vkGetPhysicalDeviceFeatures(
                _physicalDevice,
                out _features
            );

            //get family queue properties
            uint familyQueuePropertiesCount;
            VulkanNative.vkGetPhysicalDeviceQueueFamilyProperties(
                _physicalDevice,
                &familyQueuePropertiesCount,
                null
            );
            var familyQueueProperties = new NativeList<VkQueueFamilyProperties>(
                familyQueuePropertiesCount
            );
            familyQueueProperties.Count = familyQueuePropertiesCount;
            VulkanNative.vkGetPhysicalDeviceQueueFamilyProperties(
                _physicalDevice,
                &familyQueuePropertiesCount,
                (VkQueueFamilyProperties*)familyQueueProperties.Data.ToPointer()
            );

            //setup queue families
            _queueFamilies = new List<QueueFamily>();
            for (uint i = 0; i < familyQueuePropertiesCount; i++)
            {
                var familyQueueProperty = familyQueueProperties[i];
                _queueFamilies.Add(new QueueFamily(
                    i,
                    familyQueueProperty.queueCount,
                    (QueueFamilyType)familyQueueProperty.queueFlags
                ));
            }

            //get queue create infos
            var queueCreateInfos = new NativeList<VkDeviceQueueCreateInfo>(
                familyQueuePropertiesCount
            );
            queueCreateInfos.Count = familyQueuePropertiesCount;
            for (int i = 0; i < familyQueuePropertiesCount; i++)
                queueCreateInfos[i] = _queueFamilies[i].QueueCreateInfo;

            //enable extra device features
            var enabledFeatures = new VkPhysicalDeviceFeatures()
            {
                samplerAnisotropy = true
            };

            //enable swapchain extension for window support
            var enabledExtensions = new NativeList<IntPtr>();
            enabledExtensions.Add(GraphicsApiConstants.VK_KHR_SWAPCHAIN_EXTENSION_NAME);

            var deviceInfo = new VkDeviceCreateInfo
            {
                pEnabledFeatures = &enabledFeatures,
                enabledExtensionCount = enabledExtensions.Count,
                ppEnabledExtensionNames = (byte**)enabledExtensions.Data,
                enabledLayerCount = 0,
                ppEnabledLayerNames = null,
                queueCreateInfoCount = queueCreateInfos.Count,
                pQueueCreateInfos = (VkDeviceQueueCreateInfo*)queueCreateInfos.Data.ToPointer()
            };

            //setup device
            VkDevice device;
            if (VulkanNative.vkCreateDevice(
                _physicalDevice,
                &deviceInfo,
                null,
                &device
            ) != VkResult.Success)
                throw new Exception("failed to initialize device");
            _handle = device;

            //setup device queues
            foreach (var queueFamily in _queueFamilies)
                queueFamily.GetQueuesFromDevice(this);

            //calculate device score
            _score = 0;
            if (_properties.deviceType == VkPhysicalDeviceType.DiscreteGpu)
                _score += 10;
            else if (_properties.deviceType == VkPhysicalDeviceType.IntegratedGpu)
                _score += 5;
            else if (_properties.deviceType == VkPhysicalDeviceType.VirtualGpu)
                _score += 3;
            else if (_properties.deviceType == VkPhysicalDeviceType.Cpu)
                _score += 1;

            _score += (
                //1073741824 = 1024 * 1024 * 1024
                _properties.limits.maxMemoryAllocationCount / 1073741824.0f
            );
        }

        unsafe ~Device()
        {
            if (_handle != VkDevice.Null)
            {
                VulkanNative.vkDestroyDevice(
                    _handle,
                    null
                );
                _handle = VkDevice.Null;
            }
        }

        public unsafe VkFormat FindDepthFormat
        {
            get
            {
                var tiling = VkImageTiling.Optimal;
                var features = VkFormatFeatureFlags.DepthStencilAttachment;

                foreach (var format in GraphicsApiConstants.DEPTH_FORMAT_CANDIDATES)
                {
                    VkFormatProperties formatProperties;
                    VulkanNative.vkGetPhysicalDeviceFormatProperties(
                        _physicalDevice,
                        format,
                        &formatProperties
                    );

                    if (
                        tiling == VkImageTiling.Linear &&
                        (formatProperties.linearTilingFeatures & features) == features
                    )
                        return format;
                    else if (
                        tiling == VkImageTiling.Optimal &&
                        (formatProperties.optimalTilingFeatures & features) == features
                    )
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
                    if (
                        (Helpers.GetMemoryType(
                            _memoryProperties,
                            i
                        ).propertyFlags & properties
                    ) == properties)
                        return i;
                }
            }
            throw new Exception("failed to find suitable memory type on device");
        }
    }
}