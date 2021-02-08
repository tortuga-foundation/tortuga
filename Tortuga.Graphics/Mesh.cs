using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This class is used to store mesh data
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// vertex buffer 
        /// </summary>
        public API.Buffer VertexBuffer => _vertexBuffer;
        /// <summary>
        /// index buffer
        /// </summary>
        public API.Buffer IndexBuffer => _indexBuffer;

        private API.Buffer _vertexBuffer;
        private API.Buffer _indexBuffer;
        private API.Buffer _vertexBufferStaging;
        private API.Buffer _indexBufferStaging;

        /// <summary>
        /// indices of the mesh
        /// </summary>
        public ushort[] Indices;

        /// <summary>
        /// vertices of the mesh
        /// </summary>
        public Vertex[] Vertices;

        /// <summary>
        /// updates vertex buffer and index buffer, used for rendering the mesh
        /// </summary>
        public Task UpdateBuffers()
        => Task.Run(() =>
        {
            if (Indices.Length == 0)
                throw new InvalidOperationException("indices length is zero");
            if (Vertices.Length == 0)
                throw new InvalidOperationException("vertices length is zero");
            if ((Indices.Length % 3) != 0)
                throw new InvalidOperationException("indices is not a multiple of 3");

            var module = Engine.Instance.GetModule<GraphicsModule>();
            var device = module.GraphicsService.PrimaryDevice;

            var indicesSize = Convert.ToUInt32(sizeof(short) * Indices.Length);
            var verticesSize = Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * Vertices.Length);

            //only create a new index buffer if the size does not match
            if (_indexBuffer == null || _indexBuffer.Size != indicesSize)
            {
                _indexBuffer = new API.Buffer(
                    device,
                    indicesSize,
                    VkBufferUsageFlags.IndexBuffer,
                    VkMemoryPropertyFlags.DeviceLocal
                );
                _indexBufferStaging = new API.Buffer(
                    device,
                    indicesSize,
                    VkBufferUsageFlags.IndexBuffer,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent
                );
            }
            //only create a new vertex buffer if the size does not match
            if (_vertexBuffer == null || _vertexBuffer.Size != verticesSize)
            {
                _vertexBuffer = new API.Buffer(
                    device,
                    verticesSize,
                    VkBufferUsageFlags.VertexBuffer,
                    VkMemoryPropertyFlags.DeviceLocal
                );
                _vertexBufferStaging = new API.Buffer(
                    device,
                    verticesSize,
                    VkBufferUsageFlags.VertexBuffer,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent
                );
            }

            _vertexBufferStaging.SetData(Vertices);
            _indexBufferStaging.SetData(Indices);

            var fence = new API.Fence(device);
            var command = module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Transfer,
                CommandType.Primary
            );
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.CopyBuffer(_indexBufferStaging, _indexBuffer);
            command.CopyBuffer(_vertexBufferStaging, _vertexBuffer);
            command.End();
            module.CommandBufferService.Submit(
                command,
                null, null,
                fence
            );
            fence.Wait();
        });

        /// <summary>
        /// calculates tangents and bi-tangents for normal mapping
        /// </summary>
        public void ReCalculateNormals()
        {
            if (Indices.Length == 0)
                throw new InvalidOperationException("indices length is zero");
            if (Vertices.Length == 0)
                throw new InvalidOperationException("vertices length is zero");
            if ((Indices.Length % 3) != 0)
                throw new InvalidOperationException("indices is not a multiple of 3");

            for (int i = 0; i < Indices.Length; i += 3)
            {
                var v0 = Vertices[Indices[i + 0]];
                var v1 = Vertices[Indices[i + 1]];
                var v2 = Vertices[Indices[i + 2]];

                var deltaPos1 = v1.Position - v0.Position;
                var deltaPos2 = v2.Position - v0.Position;

                var deltaUV1 = v1.TextureCoordinates - v0.TextureCoordinates;
                var deltaUV2 = v2.TextureCoordinates - v0.TextureCoordinates;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                var tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                var biTangent = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;

                Vertices[Indices[i + 0]].Tangent = tangent;
                Vertices[Indices[i + 1]].Tangent = tangent;
                Vertices[Indices[i + 2]].Tangent = tangent;

                Vertices[Indices[i + 0]].BiTangent = biTangent;
                Vertices[Indices[i + 1]].BiTangent = biTangent;
                Vertices[Indices[i + 2]].BiTangent = biTangent;
            }
        }
    }
}