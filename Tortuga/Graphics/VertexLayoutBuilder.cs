using Vulkan;
using System;
using Tortuga.Graphics.API;
using System.Numerics;
using System.Collections.Generic;

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
                Byte2Norm,
                Byte2,
                SByte2,
                Byte4Norm,
                Byte4,
                SByte4Norm,
                SByte4,
                UShort2Norm,
                UShort2,
                UShort4Norm,
                UShort4,
                Short2Norm,
                Short4,
                UInt1,
                UInt2,
                UInt3,
                UInt4,
                Int1,
                Int2,
                Int3,
                Int4,
                Half1,
                Half2,
                Half4
            }

            public enum ContentType
            {
                None,
                VertexPosition,
                VertexUV,
                VertexNormal,
                ObjectPosition,
                ObjectRotation,
                ObjectScale
            }

            public ContentType Content;
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

            public static byte[] GetBytes(float val)
            {
                var totalBytes = new List<byte>();
                foreach (var b in BitConverter.GetBytes(val))
                    totalBytes.Add(b);
                return totalBytes.ToArray();
            }

            public static byte[] GetBytes(Vector2 vector)
            {
                var totalBytes = new List<byte>();
                foreach (var b in BitConverter.GetBytes(vector.X))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.Y))
                    totalBytes.Add(b);
                return totalBytes.ToArray();
            }

            public static byte[] GetBytes(Vector3 vector)
            {
                var totalBytes = new List<byte>();

                foreach (var b in BitConverter.GetBytes(vector.X))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.Y))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.Z))
                    totalBytes.Add(b);
                return totalBytes.ToArray();
            }

            public static byte[] GetBytes(Vector4 vector)
            {
                var totalBytes = new List<byte>();

                foreach (var b in BitConverter.GetBytes(vector.X))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.Y))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.Z))
                    totalBytes.Add(b);
                foreach (var b in BitConverter.GetBytes(vector.W))
                    totalBytes.Add(b);
                return totalBytes.ToArray();
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
            Bindings = new BindingElement[] { };
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