using Vulkan;
using System;
using Tortuga.Utils;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This is used to tell the pipeline what kind of vertex / instance input it should expect
    /// </summary>
    public class PipelineInputBuilder
    {
        /// <summary>
        /// Attributes the pipeline should expect
        /// </summary>
        public class AttributeElement
        {
            /// <summary>
            /// Different type of attribute format type
            /// </summary>
            public enum FormatType
            {
                /// <summary>
                /// 32-bit Float X 1
                /// </summary>
                Float1,
                /// <summary>
                /// 32-bit Float X 2
                /// </summary>
                Float2,
                /// <summary>
                /// 32-bit Float X 3
                /// </summary>
                Float3,
                /// <summary>
                /// 32-bit Float X 4
                /// </summary>
                Float4,
                /// <summary>
                /// 8-bit byte X 2
                /// </summary>
                Byte2Norm,
                /// <summary>
                /// 8-bit byte X 2
                /// </summary>
                Byte2,
                /// <summary>
                /// 8-bit signed byte X 2
                /// </summary>
                SByte2,
                /// <summary>
                /// 8-bit byte X 4
                /// </summary>
                Byte4Norm,
                /// <summary>
                /// 8-bit byte X 4
                /// </summary>
                Byte4,
                /// <summary>
                /// 8-bit signed byte X 4
                /// </summary>
                SByte4Norm,
                /// <summary>
                /// 8-bit signed byte X 4
                /// </summary>
                SByte4,
                /// <summary>
                /// 16-bit unsigned short X 2
                /// </summary>
                UShort2Norm,
                /// <summary>
                /// 16-bit unsigned short X 2
                /// </summary>
                UShort2,
                /// <summary>
                /// 16-bit unsigned short X 4
                /// </summary>
                UShort4Norm,
                /// <summary>
                /// 16-bit unsigned short X 4
                /// </summary>
                UShort4,
                /// <summary>
                /// 16-bit short X 4
                /// </summary>
                Short2Norm,
                /// <summary>
                /// 16-bit short X 4
                /// </summary>
                Short4,
                /// <summary>
                /// 32-bit uint X 1
                /// </summary>
                UInt1,
                /// <summary>
                /// 32-bit uint X 2
                /// </summary>
                UInt2,
                /// <summary>
                /// 32-bit uint X 3
                /// </summary>
                UInt3,
                /// <summary>
                /// 32-bit uint X 4
                /// </summary>
                UInt4,
                /// <summary>
                /// 32-bit int X 1
                /// </summary>
                Int1,
                /// <summary>
                /// 32-bit int X 2
                /// </summary>
                Int2,
                /// <summary>
                /// 32-bit int X 3
                /// </summary>
                Int3,
                /// <summary>
                /// 32-bit int X 4
                /// </summary>
                Int4
            }

            /// <summary>
            /// byte size of this attribute
            /// </summary>
            public uint Size => _size;
            /// <summary>
            /// attributes vulkan format
            /// </summary>
            internal VkFormat VulkanFormat => _vulkanFormat;
            private uint _size;
            private VkFormat _vulkanFormat;

            /// <summary>
            /// Constructor to create an attribute element
            /// </summary>
            /// <param name="format">Format of the attribute element</param>
            public AttributeElement(FormatType format)
            {
                switch (format)
                {
                    case FormatType.Float1:
                        _size = sizeof(float) * 1;
                        _vulkanFormat = VkFormat.R32Sfloat;
                        break;
                    case FormatType.Float2:
                        _size = sizeof(float) * 2;
                        _vulkanFormat = VkFormat.R32g32Sfloat;
                        break;
                    case FormatType.Float3:
                        _size = sizeof(float) * 3;
                        _vulkanFormat = VkFormat.R32g32b32Sfloat;
                        break;
                    case FormatType.Float4:
                        _size = sizeof(float) * 4;
                        _vulkanFormat = VkFormat.R32g32b32a32Sfloat;
                        break;
                    case FormatType.Byte2Norm:
                        _size = sizeof(byte) * 2;
                        _vulkanFormat = VkFormat.R8g8Snorm;
                        break;
                    case FormatType.Byte2:
                        _size = sizeof(byte) * 2;
                        _vulkanFormat = VkFormat.R8g8Unorm;
                        break;
                    case FormatType.SByte2:
                        _size = sizeof(byte) * 2;
                        _vulkanFormat = VkFormat.R8g8Sint;
                        break;
                    case FormatType.Byte4Norm:
                        _size = sizeof(byte) * 4;
                        _vulkanFormat = VkFormat.R8g8b8a8Unorm;
                        break;
                    case FormatType.Byte4:
                        _size = sizeof(byte) * 4;
                        _vulkanFormat = VkFormat.R8g8b8a8Uint;
                        break;
                    case FormatType.SByte4Norm:
                        _size = sizeof(byte) * 4;
                        _vulkanFormat = VkFormat.R8g8b8a8Snorm;
                        break;
                    case FormatType.SByte4:
                        _size = sizeof(byte) * 4;
                        _vulkanFormat = VkFormat.R8g8b8a8Sint;
                        break;
                    case FormatType.UShort2Norm:
                        _size = sizeof(short) * 2;
                        _vulkanFormat = VkFormat.R16g16Unorm;
                        break;
                    case FormatType.UShort2:
                        _size = sizeof(short) * 2;
                        _vulkanFormat = VkFormat.R16g16Uint;
                        break;
                    case FormatType.Short2Norm:
                        _size = sizeof(short) * 2;
                        _vulkanFormat = VkFormat.R16g16b16a16Snorm;
                        break;
                    case FormatType.UShort4Norm:
                        _size = sizeof(short) * 4;
                        _vulkanFormat = VkFormat.R16g16b16a16Unorm;
                        break;
                    case FormatType.UShort4:
                        _size = sizeof(short) * 4;
                        _vulkanFormat = VkFormat.R16g16b16a16Uint;
                        break;
                    case FormatType.Short4:
                        _size = sizeof(short) * 4;
                        _vulkanFormat = VkFormat.R16g16b16a16Sint;
                        break;
                    case FormatType.UInt1:
                        _size = sizeof(uint);
                        _vulkanFormat = VkFormat.R32Uint;
                        break;
                    case FormatType.UInt2:
                        _size = sizeof(uint) * 2;
                        _vulkanFormat = VkFormat.R32g32Uint;
                        break;
                    case FormatType.UInt3:
                        _size = sizeof(uint) * 2;
                        _vulkanFormat = VkFormat.R32g32b32Uint;
                        break;
                    case FormatType.UInt4:
                        _size = sizeof(uint) * 2;
                        _vulkanFormat = VkFormat.R32g32b32a32Uint;
                        break;
                    case FormatType.Int1:
                        _size = sizeof(int) * 2;
                        _vulkanFormat = VkFormat.R32Sint;
                        break;
                    case FormatType.Int2:
                        _size = sizeof(int) * 2;
                        _vulkanFormat = VkFormat.R32g32Sint;
                        break;
                    case FormatType.Int3:
                        _size = sizeof(int) * 2;
                        _vulkanFormat = VkFormat.R32g32b32Sint;
                        break;
                    case FormatType.Int4:
                        _size = sizeof(int) * 2;
                        _vulkanFormat = VkFormat.R32g32b32a32Sint;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        };

        /// <summary>
        /// Bindings the pipeline should expect
        /// </summary>
        public class BindingElement
        {
            /// <summary>
            /// Different type of binding rates
            /// </summary>
            public enum BindingType
            {
                /// <summary>
                /// Vertex Rate for the binding
                /// </summary>
                Vertex,
                /// <summary>
                /// Instance Rate for the binding
                /// </summary>
                Instance
            };

            /// <summary>
            /// The type of the binding
            /// </summary>
            public BindingType Type;
            /// <summary>
            /// The attributes for this binding
            /// </summary>
            public AttributeElement[] Elements;
            /// <summary>
            /// Total size of this binding 
            /// </summary>
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

        /// <summary>
        /// Total bindings for this pipeline input builder
        /// </summary>
        public BindingElement[] Bindings;

        /// <summary>
        /// An empty pipeline input builder
        /// </summary>
        public PipelineInputBuilder()
        {
            Bindings = new BindingElement[] { };
        }

        /// <summary>
        /// Create a pipeline builder
        /// </summary>
        /// <param name="bindings">The bindings the pipeline should expect</param>
        public PipelineInputBuilder(BindingElement[] bindings)
        {
            Bindings = bindings;
        }

        /// <summary>
        /// Used to build the pipeline
        /// </summary>
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

        /// <summary>
        /// Used to build the pipeline
        /// </summary>
        internal NativeList<VkVertexInputAttributeDescription> AttributeDescriptions
        {
            get
            {
                var attributeDescriptions = new NativeList<VkVertexInputAttributeDescription>();
                uint location = 0;
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
                            location = location,
                            offset = offset
                        });
                        offset += element.Size;
                        location++;
                    }
                }
                return attributeDescriptions;
            }
        }
    }
}