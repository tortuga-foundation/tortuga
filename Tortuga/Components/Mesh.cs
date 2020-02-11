using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;

namespace Tortuga.Components
{
    public class Mesh : Core.BaseComponent
    {
        public Material ActiveMaterial => _material;
        internal CommandPool.Command RenderCommand => _renderCommand;

        private Material _material;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;

        public override void OnEnable()
        {
            if (_material == null)
                _material = new Material("Assets/Shaders/Simple.vert.spv", "Assets/Shaders/Simple.frag.spv");
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
        }
    }
}