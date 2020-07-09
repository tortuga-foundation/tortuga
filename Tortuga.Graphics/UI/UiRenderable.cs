#pragma warning disable 1591
using System;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics;
using Vulkan;
using Tortuga.Input;

namespace Tortuga.UI
{
    /// <summary>
    /// Renderable ui element
    /// </summary>
    public class UiRenderable : UiElement
    {
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
        public Camera RenderFromCamera = null;

        public bool IsMouseInside
        {
            get
            {
                var pos = AbsolutePosition;
                var scale = Scale;
                var mousePosition = InputModule.MousePosition;
                return (
                    mousePosition.X >= pos.X &&
                    mousePosition.Y >= pos.Y &&
                    mousePosition.X <= scale.X + pos.X &&
                    mousePosition.Y <= scale.Y + pos.Y
                );
            }
        }

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
            if (RenderFromCamera != null)
            {
                _descriptorHelper.BindImage(
                    TEXTURE_KEY, 0,
                    RenderFromCamera.DefferedFramebuffer.AttachmentImages[0]
                );
            }

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
                            Color = new Vector4(
                                Background.R / 255.0f,
                                Background.G / 255.0f,
                                Background.B / 255.0f,
                                Background.A / 255.0f
                            )
                        }
                    }
                )
            };
        }

        internal virtual Graphics.API.CommandPool.Command Draw(Graphics.API.Framebuffer frameBuffer, Graphics.API.DescriptorSetPool.DescriptorSet ProjectionDescriptorSet)
        {
            _command.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                UiResources.Instance.RenderPass,
                frameBuffer
            );
            var scissorPosition = AbsolutePosition;
            var scissorScale = Scale;
            if (Mask != null)
            {
                //update position with mask
                var maskPosition = Mask.AbsolutePosition;
                if (scissorPosition.X > maskPosition.X)
                    scissorPosition.X = maskPosition.X;
                if (scissorPosition.Y > maskPosition.Y)
                    scissorPosition.Y = maskPosition.Y;

                //update scale with mask
                if (scissorScale.X < Mask.Scale.X)
                    scissorScale.X = Mask.Scale.X;
                if (scissorScale.Y < Mask.Scale.Y)
                    scissorScale.Y = Mask.Scale.Y;
            }
            if (scissorScale.X > 0 && scissorScale.Y > 0)
            {
                _command.BindPipeline(UiResources.Instance.Pipeline);
                _command.BindDescriptorSets(
                    UiResources.Instance.Pipeline,
                    new Graphics.API.DescriptorSetPool.DescriptorSet[]
                    {
                    ProjectionDescriptorSet,
                    _descriptorHelper.DescriptorObjectMapper[DATA_KEY].Set,
                    _descriptorHelper.DescriptorObjectMapper[TEXTURE_KEY].Set,
                    }
                );
                _command.SetScissor(
                    Convert.ToInt32(scissorPosition.X),
                    Convert.ToInt32(scissorPosition.Y),
                    Convert.ToUInt32(scissorScale.X),
                    Convert.ToUInt32(scissorScale.Y)
                );
                _command.SetViewport(
                    0, 0,
                    frameBuffer.Width,
                    frameBuffer.Height
                );
                _command.Draw(6);
            }
            _command.End();
            return _command;
        }
        public Task SetTexture(ShaderPixel pixel)
        {
            if (RenderFromCamera != null)
            {
                return Task.Run(() =>
                {
                    _descriptorHelper.BindImage(
                        TEXTURE_KEY, 0,
                        RenderFromCamera.DefferedFramebuffer.AttachmentImages[0]
                    );
                });
            }
            _isDirty = true;
            return _descriptorHelper.BindImage(TEXTURE_KEY, 0, new ShaderPixel[] { pixel }, 1, 1);
        }
        public Task SetTexture(ShaderPixel[] pixels, int width, int height)
        {
            if (RenderFromCamera != null)
            {
                return Task.Run(() =>
                {
                    _descriptorHelper.BindImage(
                        TEXTURE_KEY, 0,
                        RenderFromCamera.DefferedFramebuffer.AttachmentImages[0]
                    );
                });
            }
            _isDirty = true;
            return _descriptorHelper.BindImage(TEXTURE_KEY, 0, pixels, width, height);
        }
    }
}