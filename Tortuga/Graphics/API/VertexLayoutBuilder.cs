using Vulkan;
using System.Numerics;

namespace Tortuga.Graphics
{
    [System.Serializable]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinates;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;

        public static Vertex[] ComputeTangents(Vertex[] vertices, uint[] indices)
        {
            //compute tangent & bi tangents
            for (uint i = 0; i < indices.Length; i += 3)
            {
                var v0 = vertices[indices[i + 0]];
                var v1 = vertices[indices[i + 1]];
                var v2 = vertices[indices[i + 2]];

                var edge1 = v1.Position - v0.Position;
                var edge2 = v2.Position - v0.Position;

                float deltaU1 = v1.TextureCoordinates.X - v0.TextureCoordinates.X;
                float deltaV1 = v1.TextureCoordinates.Y - v0.TextureCoordinates.Y;
                float deltaU2 = v2.TextureCoordinates.X - v0.TextureCoordinates.X;
                float deltaV2 = v2.TextureCoordinates.Y - v0.TextureCoordinates.Y;

                float f = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);
                Vector3 tangent, bitangent;
                tangent.X = f * (deltaV2 * edge1.X - deltaV1 * edge2.X);
                tangent.Y = f * (deltaV2 * edge1.Y - deltaV1 * edge2.Y);
                tangent.Z = f * (deltaV2 * edge1.Z - deltaV1 * edge2.Z);

                bitangent.X = f * (-deltaU2 * edge1.X - deltaU1 * edge2.X);
                bitangent.Y = f * (-deltaU2 * edge1.Y - deltaU1 * edge2.Y);
                bitangent.Z = f * (-deltaU2 * edge1.Z - deltaU1 * edge2.Z);

                vertices[indices[i + 0]].Tangent += tangent;
                vertices[indices[i + 1]].Tangent += tangent;
                vertices[indices[i + 2]].Tangent += tangent;

                vertices[indices[i + 0]].BiTangent += bitangent;
                vertices[indices[i + 1]].BiTangent += bitangent;
                vertices[indices[i + 2]].BiTangent += bitangent;
            }
            //normalize tangents
            for (uint i = 0; i < vertices.Length; i++)
            {
                vertices[i].Tangent = Vector3.Normalize(vertices[i].Tangent);
                vertices[i].BiTangent = Vector3.Normalize(vertices[i].BiTangent);
            }
            return vertices;
        }
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
            },
            //bi-tangent
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