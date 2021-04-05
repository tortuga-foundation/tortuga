#pragma warning disable CS1591
using System;

namespace Vulkan
{
    public static partial class Helpers
    {
        public static VkMemoryType GetMemoryType(
            VkPhysicalDeviceMemoryProperties memoryProperties,
            uint i
        )
        {
            switch (i)
            {
                case 0:
                    return memoryProperties.memoryTypes_0;
                case 1:
                    return memoryProperties.memoryTypes_1;
                case 2:
                    return memoryProperties.memoryTypes_2;
                case 3:
                    return memoryProperties.memoryTypes_3;
                case 4:
                    return memoryProperties.memoryTypes_4;
                case 5:
                    return memoryProperties.memoryTypes_5;
                case 6:
                    return memoryProperties.memoryTypes_6;
                case 7:
                    return memoryProperties.memoryTypes_7;
                case 8:
                    return memoryProperties.memoryTypes_8;
                case 9:
                    return memoryProperties.memoryTypes_9;
                case 10:
                    return memoryProperties.memoryTypes_10;
                case 11:
                    return memoryProperties.memoryTypes_11;
                case 12:
                    return memoryProperties.memoryTypes_12;
                case 13:
                    return memoryProperties.memoryTypes_13;
                case 14:
                    return memoryProperties.memoryTypes_14;
                case 15:
                    return memoryProperties.memoryTypes_15;
                case 16:
                    return memoryProperties.memoryTypes_16;
                case 17:
                    return memoryProperties.memoryTypes_17;
                case 18:
                    return memoryProperties.memoryTypes_18;
                case 19:
                    return memoryProperties.memoryTypes_19;
                case 20:
                    return memoryProperties.memoryTypes_20;
                case 21:
                    return memoryProperties.memoryTypes_21;
                case 22:
                    return memoryProperties.memoryTypes_22;
                case 23:
                    return memoryProperties.memoryTypes_23;
                case 24:
                    return memoryProperties.memoryTypes_24;
                case 25:
                    return memoryProperties.memoryTypes_25;
                case 26:
                    return memoryProperties.memoryTypes_26;
                case 27:
                    return memoryProperties.memoryTypes_27;
                case 28:
                    return memoryProperties.memoryTypes_28;
                case 29:
                    return memoryProperties.memoryTypes_29;
                case 30:
                    return memoryProperties.memoryTypes_30;
                case 31:
                    return memoryProperties.memoryTypes_31;
                default:
                    throw new NotSupportedException("this type of memory is not supported");
            }
        }
    }
}