using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;

namespace Tortuga.Components
{
    public class Camera : Core.BaseComponent
    {
        internal Framebuffer Framebuffer => _framebuffer;

        private Framebuffer _framebuffer;

        public override void OnEnable()
        {
            _framebuffer = new Framebuffer(
                Engine.Instance.MainRenderPass,
                1920, 1080
            );
        }
    }
}