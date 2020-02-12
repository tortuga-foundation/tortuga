using Vulkan;

namespace Tortuga.Graphics
{
    [System.Serializable]
    public struct Vertex
    {
        public Tortuga.Math.Vector3 Position;
        public Tortuga.Math.Vector2 TextureCoordinates;
        public Tortuga.Math.Vector3 Normals;
        public Tortuga.Math.Vector3 Tangents;
    }
}

namespace Tortuga.Graphics.API
{
    internal class VertexLayoutBuilder
    {
        private class VertexElement
        {
            public VkFormat Format;
            public uint Size;
        };
        private readonly static VertexElement[] _elements = new VertexElement[]
        {
            //position
            new VertexElement{
                Format = VkFormat.R32g32b32Sfloat,
                Size = sizeof(float) * 3
            },
            //texture coordinate
            new VertexElement{
                Format = VkFormat.R32g32Sfloat,
                Size = sizeof(float) * 2
            },
            //normals
            new VertexElement{
                Format = VkFormat.R32g32b32Sfloat,
                Size = sizeof(float) * 3
            },
            //tangent
            new VertexElement{
                Format = VkFormat.R32g32b32Sfloat,
                Size = sizeof(float) * 3
            }
        };
        public static uint Size
        {
            get
            {
                uint size = 0;
                foreach (var s in _elements)
                    size += s.Size;
                return size;
            }
        }
        public static NativeList<VkVertexInputBindingDescription> BindingDescriptions
        {
            get
            {
                var bindingDescriptions = new NativeList<VkVertexInputBindingDescription>();
                bindingDescriptions.Add(new VkVertexInputBindingDescription
                {
                    binding = 0,
                    stride = VertexLayoutBuilder.Size,
                    inputRate = VkVertexInputRate.Vertex
                });
                return bindingDescriptions;
            }
        }

        public static NativeList<VkVertexInputAttributeDescription> AttributeDescriptions
        {
            get
            {
                var attributeDescriptions = new NativeList<VkVertexInputAttributeDescription>();
                uint offset = 0;
                for (uint i = 0; i < _elements.Length; i++)
                {
                    var element = _elements[i];
                    attributeDescriptions.Add(new VkVertexInputAttributeDescription
                    {
                        binding = 0,
                        format = element.Format,
                        location = i,
                        offset = offset
                    });
                    offset += element.Size;
                }
                return attributeDescriptions;
            }
        }

        private VertexLayoutBuilder() { }
    }
}