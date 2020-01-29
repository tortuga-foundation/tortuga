using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vulkan;
using static Vulkan.VulkanNative;

public class Device
{
    public class DeviceQueueFamilyIndex
    {
        public uint Index;
        public uint Count;
    }
    public class DeviceQueueFamilies
    {
        public DeviceQueueFamilyIndex Compute;
        public DeviceQueueFamilyIndex Graphics;
        public DeviceQueueFamilyIndex Transfer;
    }
    public class DeviceQueues
    {
        public List<VkQueue> Compute;
        public List<VkQueue> Graphics;
        public List<VkQueue> Transfer;
    }

    public VkPhysicalDevice PhysicalDevice => _physicalDevice;
    public VkDevice LogicalDevice => _device;
    public VkPhysicalDeviceProperties Properties => _properties;
    public VkPhysicalDeviceFeatures Features => _features;
    public VkPhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;
    public DeviceQueueFamilies QueueFamilies => _queueFamilies;
    public DeviceQueues Queues => _queues;

    private VkPhysicalDevice _physicalDevice;
    private VkPhysicalDeviceProperties _properties;
    private VkPhysicalDeviceFeatures _features;
    private VkPhysicalDeviceMemoryProperties _memoryProperties;
    private VkDevice _device;
    private DeviceQueueFamilies _queueFamilies;
    public DeviceQueues _queues;

    private unsafe DeviceQueueFamilies FindDeviceQueueIndices()
    {
        uint queueFamilyCount = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, null);
        var queueFamilies = new NativeList<VkQueueFamilyProperties>(queueFamilyCount);
        vkGetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, (VkQueueFamilyProperties*)queueFamilies.Data.ToPointer());
        queueFamilies.Count = queueFamilyCount;

        int bestCompute = -1;
        int bestGraphics = -1;
        int bestTransfer = -1;

        int compute = -1;
        int graphics = -1;
        int transfer = -1;

        var data = new DeviceQueueFamilies();
        data.Graphics = new DeviceQueueFamilyIndex();
        data.Compute = new DeviceQueueFamilyIndex();
        data.Transfer = new DeviceQueueFamilyIndex();
        for (int i = 0; i < queueFamilyCount; i++)
        {
            var queueFamily = queueFamilies[i];
            if (queueFamily.queueCount <= 0)
                continue;

            if ((queueFamily.queueFlags & VkQueueFlags.Compute) != 0)
            {
                if (
                    (queueFamily.queueFlags & VkQueueFlags.Graphics) == 0 &&
                    (queueFamily.queueFlags & VkQueueFlags.Transfer) == 0
                )
                    bestCompute = i;
                else
                    compute = i;

                data.Compute.Count = queueFamily.queueCount;
            }
            if ((queueFamily.queueFlags & VkQueueFlags.Graphics) != 0)
            {
                if (
                    (queueFamily.queueFlags & VkQueueFlags.Compute) == 0 &&
                    (queueFamily.queueFlags & VkQueueFlags.Transfer) == 0
                )
                    bestGraphics = i;
                else
                    graphics = i;

                data.Graphics.Count = queueFamily.queueCount;
            }
            if ((queueFamily.queueFlags & VkQueueFlags.Transfer) != 0)
            {
                if (
                    (queueFamily.queueFlags & VkQueueFlags.Compute) == 0 &&
                    (queueFamily.queueFlags & VkQueueFlags.Graphics) == 0
                )
                    bestTransfer = i;
                else
                    transfer = i;

                data.Transfer.Count = queueFamily.queueCount;
            }
        }
        if (bestCompute > -1)
            data.Compute.Index = Convert.ToUInt32(bestCompute);
        else if (compute > -1)
            data.Compute.Index = Convert.ToUInt32(compute);
        if (bestGraphics > -1)
            data.Graphics.Index = Convert.ToUInt32(bestGraphics);
        else if (graphics > -1)
            data.Graphics.Index = Convert.ToUInt32(graphics);
        if (bestTransfer > -1)
            data.Transfer.Index = Convert.ToUInt32(bestTransfer);
        else if (transfer > -1)
            data.Transfer.Index = Convert.ToUInt32(transfer);
        return data;
    }

    public unsafe Device(VkPhysicalDevice physicalDevice)
    {
        this._physicalDevice = physicalDevice;

        VkPhysicalDeviceProperties properties;
        vkGetPhysicalDeviceProperties(_physicalDevice, out properties);
        _properties = properties;

        VkPhysicalDeviceFeatures features;
        vkGetPhysicalDeviceFeatures(_physicalDevice, out features);
        _features = features;

        VkPhysicalDeviceMemoryProperties memoryProperties;
        vkGetPhysicalDeviceMemoryProperties(_physicalDevice, out memoryProperties);
        _memoryProperties = memoryProperties;

        this._queueFamilies = this.FindDeviceQueueIndices();

        var familyIndices = new List<DeviceQueueFamilyIndex>();
        if (familyIndices.FindIndex(f => f.Index == this._queueFamilies.Compute.Index) == -1)
            familyIndices.Add(this._queueFamilies.Compute);
        if (familyIndices.FindIndex(f => f.Index == this._queueFamilies.Graphics.Index) == -1)
            familyIndices.Add(this._queueFamilies.Graphics);
        if (familyIndices.FindIndex(f => f.Index == this._queueFamilies.Transfer.Index) == -1)
            familyIndices.Add(this._queueFamilies.Transfer);
        var queuePriority = new NativeList<float>[familyIndices.Count];
        var familyIndicesCountUint = Convert.ToUInt32(familyIndices.Count);
        var queueCreateInfos = new NativeList<VkDeviceQueueCreateInfo>(familyIndicesCountUint);
        queueCreateInfos.Count = familyIndicesCountUint;
        for (int i = 0; i < familyIndices.Count; i++)
        {
            queuePriority[i] = new NativeList<float>(familyIndices[i].Count);
            for (int j = 0; j < queuePriority[i].Count; j++)
                queuePriority[i][j] = 1.0f / queuePriority[i].Count;

            queueCreateInfos[i] = VkDeviceQueueCreateInfo.New();
            queueCreateInfos[i].queueFamilyIndex = familyIndices[i].Index;
            queueCreateInfos[i].queueCount = familyIndices[i].Count;
            queueCreateInfos[i].pQueuePriorities = (float*)queuePriority[i].Data.ToPointer();
        }

        var enabledDeviceFeatures = new VkPhysicalDeviceFeatures();
        enabledDeviceFeatures.samplerAnisotropy = VkBool32.True;

        var deviceExtensions = new NativeList<IntPtr>();
        deviceExtensions.Add(Strings.VK_KHR_SWAPCHAIN_EXTENSION_NAME);

        var deviceInfo = VkDeviceCreateInfo.New();
        deviceInfo.pEnabledFeatures = &enabledDeviceFeatures;
        deviceInfo.queueCreateInfoCount = queueCreateInfos.Count;
        deviceInfo.pQueueCreateInfos = (VkDeviceQueueCreateInfo*)queueCreateInfos.Data.ToPointer();
        deviceInfo.enabledExtensionCount = deviceExtensions.Count;
        deviceInfo.ppEnabledExtensionNames = (byte**)deviceExtensions.Data;
        deviceInfo.enabledLayerCount = 0;

        VkDevice logicalDevice;
        if (vkCreateDevice(this.PhysicalDevice, &deviceInfo, null, &logicalDevice) != VkResult.Success)
            throw new Exception("Failed to initialize device");
        this._device = logicalDevice;

        if (this.QueueFamilies.Compute.Count == 0 || this.QueueFamilies.Graphics.Count == 0 || this.QueueFamilies.Transfer.Count == 0)
            throw new Exception("This device does not support compute, graphics or transfer queue");

        this._queues = new DeviceQueues();
        this._queues.Compute = new List<VkQueue>();
        this._queues.Graphics = new List<VkQueue>();
        this._queues.Transfer = new List<VkQueue>();
        for (uint i = 0; i < this.QueueFamilies.Compute.Count; i++)
        {
            VkQueue queue;
            vkGetDeviceQueue(this._device, this.QueueFamilies.Compute.Index, i, &queue);
            this._queues.Compute.Add(queue);
        }
        for (uint i = 0; i < this.QueueFamilies.Graphics.Count; i++)
        {
            if (this.QueueFamilies.Compute.Index == this.QueueFamilies.Graphics.Index)
                this._queues.Graphics.Add(this._queues.Compute[Convert.ToInt32(i)]);
            else
            {
                VkQueue queue;
                vkGetDeviceQueue(this._device, this.QueueFamilies.Graphics.Index, i, &queue);
                this._queues.Graphics.Add(queue);
            }
        }
        for (uint i = 0; i < this.QueueFamilies.Transfer.Count; i++)
        {
            if (this.QueueFamilies.Compute.Index == this.QueueFamilies.Transfer.Index)
                this._queues.Transfer.Add(this._queues.Compute[Convert.ToInt32(i)]);
            else if (this.QueueFamilies.Graphics.Index == this.QueueFamilies.Transfer.Index)
                this._queues.Transfer.Add(this._queues.Graphics[Convert.ToInt32(i)]);
            else
            {
                VkQueue queue;
                vkGetDeviceQueue(this._device, this.QueueFamilies.Transfer.Index, i, &queue);
                this._queues.Transfer.Add(queue);
            }
        }
    }
}