#pragma warning disable 1591
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics;
using Vulkan;

namespace Tortuga.UI
{
    /// <summary>
    /// Renderable ui element
    /// </summary>
    public class UiRenderable : UiElement
    {
        public Vector4 Color = new Vector4(1, 1, 1, 1);
        private const string DATA_KEY = "DATA";
        private const string TEXTURE_KEY = "TEXTURE";

        private struct RenderData
        {
            public Vector2 Position;
            public Vector2 Scale;
            public Vector4 BorderRadius;
            public Vector4 Color;
        }
        private DescriptorSetHelper _descriptorHelper;
        private Graphics.API.CommandPool _commandPool;
        private Graphics.API.CommandPool.Command _command;

        public UiRenderable()
        {
            //setup descriptor set buffers
            _descriptorHelper = new DescriptorSetHelper();
            _descriptorHelper.InsertKey(DATA_KEY, UiResources.Instance.DescriptorSetLayouts[1]);
            _descriptorHelper.InsertKey(TEXTURE_KEY, UiResources.Instance.DescriptorSetLayouts[2]);
            _descriptorHelper.BindBuffer(DATA_KEY, 0, new RenderData[]
                {
                    new RenderData()
                    {
                        Position = AbsolutePosition,
                        Scale = this.Scale,
                        BorderRadius = Vector4.Zero
                    }
                }
            ).Wait();
            _descriptorHelper.BindImage(TEXTURE_KEY, 0, new ShaderPixel[] { ShaderPixel.White }, 1, 1).Wait();

            //setup render command
            _commandPool = new Graphics.API.CommandPool(
                Graphics.API.Handler.MainDevice,
                Graphics.API.Handler.MainDevice.GraphicsQueueFamily
            );
            _command = _commandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
        }

        internal virtual Graphics.API.BufferTransferObject[] CreateOrUpdateBuffers()
        {
            if (_isDirty == false)
                return new Graphics.API.BufferTransferObject[] { };

            _isDirty = false;
            return new Graphics.API.BufferTransferObject[]
            {
                _descriptorHelper.BindBufferWithTransferObject(DATA_KEY, 0, new RenderData[]
                    {
                        new RenderData()
                        {
                            Position = AbsolutePosition,
                            Scale = this.Scale,
                            BorderRadius = new Vector4(
                                BorderRadiusTopLeft,
                                BorderRadiusTopRight,
                                BorderRadiusBottomLeft,
                                BorderRadiusBottomRight
                            ),
                            Color = Color
                        }
                    }
                )
            };
        }

        internal virtual Graphics.API.CommandPool.Command Draw(Camera camera)
        {
            _command.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                UiResources.Instance.RenderPass,
                camera.DefferedFramebuffer
            );
            _command.BindPipeline(UiResources.Instance.Pipeline);
            _command.BindDescriptorSets(
                UiResources.Instance.Pipeline,
                new Graphics.API.DescriptorSetPool.DescriptorSet[]
                {
                    camera.UiProjecionSet,
                    _descriptorHelper.DescriptorObjectMapper[DATA_KEY].Set,
                    _descriptorHelper.DescriptorObjectMapper[TEXTURE_KEY].Set,
                }
            );
            _command.SetScissor(
                0, 0,
                camera.DefferedFramebuffer.Width,
                camera.DefferedFramebuffer.Height
            );
            _command.SetViewport(
                0, 0,
                camera.DefferedFramebuffer.Width,
                camera.DefferedFramebuffer.Height
            );
            _command.Draw(6);
            _command.End();
            return _command;
        }

        public Task SetTexture(ShaderPixel pixel)
        {
            _isDirty = true;
            return _descriptorHelper.BindImage(TEXTURE_KEY, 0, new ShaderPixel[] { pixel }, 1, 1);
        }
        public Task SetTexture(ShaderPixel[] pixels, int width, int height)
        {
            _isDirty = true;
            return _descriptorHelper.BindImage(TEXTURE_KEY, 0, pixels, width, height);
        }
    }
}