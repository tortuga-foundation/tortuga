using System;
using System.Threading.Tasks;
using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;

namespace Tortuga.Components
{
    public class Mesh : Core.BaseComponent
    {
        public Material ActiveMaterial => _material;
        internal CommandPool.Command RenderCommand => _renderCommand;
        internal Graphics.API.Buffer VertexBuffer => _vertexBuffer;
        internal Graphics.API.Buffer IndexBuffer => _indexBuffer;
        internal uint IndicesCount => _indicesCount;

        private Material _material;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Graphics.API.Buffer _vertexBuffer;
        private Graphics.API.Buffer _indexBuffer;
        private uint _indicesCount = 3;

        public async override Task OnEnable()
        {
            if (_material == null)
                _material = new Material("Assets/Shaders/Simple.vert.spv", "Assets/Shaders/Simple.frag.spv");
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            await SetVertices(new Vertex[]{
                new Vertex{
                    Position = new Math.Vector3(0, 1, 0),
                },
                new Vertex{
                    Position = new Math.Vector3(-1, 0, 0),
                },
                new Vertex{
                    Position = new Math.Vector3(1, 0, 0),
                }
            });
            await SetInices(new uint[] { 0, 1, 2 });
        }

        public async Task SetVertices(Vertex[] vertices)
        {
            unsafe
            {
                _vertexBuffer = Graphics.API.Buffer.CreateDevice(
                    Convert.ToUInt32(sizeof(Vertex) * vertices.Length),
                    VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst
                );
            }
            await _vertexBuffer.SetDataWithStaging(vertices);
        }
        public async Task SetInices(uint[] indices)
        {
            _indexBuffer = Graphics.API.Buffer.CreateDevice(
                Convert.ToUInt32(sizeof(uint) * indices.Length),
                VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst
            );
            _indicesCount = Convert.ToUInt32(indices.Length);
            await _indexBuffer.SetDataWithStaging(indices);
        }
    }
}