using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;
using System;

namespace Tortuga.Components
{
    public class Camera : Core.BaseComponent
    {
        internal Framebuffer Framebuffer => _framebuffer;
        public Tortuga.Math.Rect Viewport = new Math.Rect
        {
            x = 0,
            y = 0,
            width = 1,
            height = 1
        };
        public Tortuga.Math.Int2D Resolution
        {
            get
            {
                return new Math.Int2D
                {
                    x = Convert.ToInt32(_framebuffer.Width),
                    y = Convert.ToInt32(_framebuffer.Height)
                };
            }
            set
            {
                _framebuffer = new Framebuffer(
                    Engine.Instance.MainRenderPass,
                    Convert.ToUInt32(value.x), Convert.ToUInt32(value.y)
                );
            }
        }

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