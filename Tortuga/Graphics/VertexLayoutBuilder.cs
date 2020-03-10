using Vulkan;
using System;
using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    public class PipelineInputBuilder
    {
        public class AttributeElement
        {
            public enum FormatType
            {
                Float1,
                Float2,
                Float3,
                Float4,
                VertexPosition,
                VertexUV,
                VertexNormal,
                ObjectPosition,
                ObjectRotation,
                ObjectScale
            }

            public FormatType Format => _format;
            private FormatType _format;
            public uint Size => _size;
            private uint _size;
            internal VkFormat VulkanFormat => _vulkanFormat;
            private VkFormat _vulkanFormat;

            public AttributeElement(FormatType format)
            {
                _format = format;
                switch (format)
                {
                    case FormatType.Float1:
                        _size = sizeof(float) * 1;
                        _vulkanFormat = VkFormat.R32Sfloat;
                        break;
                    case FormatType.VertexUV:
                    case FormatType.Float2:
                        _size = sizeof(float) * 2;
                        _vulkanFormat = VkFormat.R32g32Sfloat;
                        break;
                    case FormatType.ObjectPosition:
                    case FormatType.ObjectRotation:
                    case FormatType.ObjectScale:
                    case FormatType.VertexPosition:
                    case FormatType.VertexNormal:
                    case FormatType.Float3:
                        _size = sizeof(float) * 3;
                        _vulkanFormat = VkFormat.R32g32b32Sfloat;
                        break;
                    case FormatType.Float4:
                        _size = sizeof(float) * 4;
                        _vulkanFormat = VkFormat.R32g32b32a32Sfloat;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        };
        public class BindingElement
        {
            public enum BindingType
            {
                Vertex,
                Instance
            };

            public BindingType Type;
            public AttributeElement[] Elements;
            public uint Size
            {
                get
                {
                    uint size = 0;
                    foreach (var e in Elements)
                        size += e.Size;
                    return size;
                }
            }
        }
        public BindingElement[] Bindings;

        public PipelineInputBuilder()
        {
            Bindings = new BindingElement[]{};
        }
        public PipelineInputBuilder(BindingElement[] bindings)
        {
            Bindings = bindings;
        }

        internal NativeList<VkVertexInputBindingDescription> BindingDescriptions
        {
            get
            {
                var bindingDescriptions = new NativeList<VkVertexInputBindingDescription>();
                for (uint i = 0; i < Bindings.Length; i++)
                {
                    VkVertexInputRate type;
                    if (Bindings[i].Type == BindingElement.BindingType.Vertex)
                        type = VkVertexInputRate.Vertex;
                    else if (Bindings[i].Type == BindingElement.BindingType.Instance)
                        type = VkVertexInputRate.Instance;
                    else
                        throw new NotSupportedException();

                    bindingDescriptions.Add(new VkVertexInputBindingDescription
                    {
                        binding = i,
                        stride = Bindings[i].Size,
                        inputRate = type
                    });
                }
                return bindingDescriptions;
            }
        }

        internal NativeList<VkVertexInputAttributeDescription> AttributeDescriptions
        {
            get
            {
                var attributeDescriptions = new NativeList<VkVertexInputAttributeDescription>();
                for (uint i = 0; i < Bindings.Length; i++)
                {
                    uint offset = 0;
                    for (uint j = 0; j < Bindings[i].Elements.Length; j++)
                    {
                        var element = Bindings[i].Elements[j];
                        attributeDescriptions.Add(new VkVertexInputAttributeDescription
                        {
                            binding = i,
                            format = element.VulkanFormat,
                            location = j,
                            offset = offset
                        });
                        offset += element.Size;
                    }
                }
                return attributeDescriptions;
            }
        }
    }
}