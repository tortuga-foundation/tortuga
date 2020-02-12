using System;
using System.Threading.Tasks;
using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;
using System.Runtime.CompilerServices;

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

            _vertexBuffer = Graphics.API.Buffer.CreateHost(
                Convert.ToUInt32(Unsafe.SizeOf<Vertex>()) * 3,
                VkBufferUsageFlags.VertexBuffer
            );
            _indexBuffer = Graphics.API.Buffer.CreateHost(
                sizeof(uint) * 3,
                VkBufferUsageFlags.IndexBuffer
            );
            _vertexBuffer.SetData(new Vertex[]{
                new Vertex(){ Position = new Math.Vector3(0, -0.5f, 0) },
                new Vertex(){ Position = new Math.Vector3(-0.5f, 0, 0) },
                new Vertex(){ Position = new Math.Vector3(0.5f, 0, 0) }
            });
            _indexBuffer.SetData(new uint[] { 0, 2, 1 });
        }
    }
}