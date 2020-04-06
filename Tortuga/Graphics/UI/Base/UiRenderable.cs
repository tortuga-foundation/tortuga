using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics.UI.Base
{
    /// <summary>
    /// Renderable Ui Component
    /// </summary>
    public class UiRenderable : UiElement
    {
        private struct ShaderData
        {
            public Vector2 Position;
            public Vector2 Scale;
            public Vector4 Color;
            public Vector4 BorderRadius;

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return base.ToString();
            }

            public static bool operator ==(ShaderData A, ShaderData B)
            {
                return (
                    A.Position == B.Position &&
                    A.Scale == B.Scale &&
                    A.Color == B.Color &&
                    A.BorderRadius == B.BorderRadius
                );
            }
            public static bool operator !=(ShaderData A, ShaderData B)
            {
                return (
                    A.Position != B.Position ||
                    A.Scale != B.Scale ||
                    A.Color != B.Color ||
                    A.BorderRadius != B.BorderRadius
                );
            }
        }

        /// <summary>
        /// If element is outside mask then it will not be rendered
        /// </summary>
        public UiElement Mask
        {
            get => _mask;
            set
            {
                foreach (var child in this.Children)
                {
                    var renderChild = child as UiRenderable;
                    if (renderChild != null)
                        renderChild.Mask = value;
                }
                _mask = value;
            }
        }
        private UiElement _mask;

        /// <summary>
        /// Material used for rendering
        /// </summary>
        protected UiMaterial _material;

        internal API.DescriptorSetPool.DescriptorSet DescriptorSet => _descriptorSet;
        internal API.CommandPool.Command RenderCommand => _renderCommand;
        private API.DescriptorSetPool _descriptorPool;
        private API.DescriptorSetPool.DescriptorSet _descriptorSet;
        private API.Buffer _descriptorbuffer;

        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;

        /// <summary>
        /// Constructor for renderable Ui Component
        /// </summary>
        public UiRenderable()
        {
            //setup base descriptor buffer
            _descriptorbuffer = API.Buffer.CreateDevice(
                System.Convert.ToUInt32(Unsafe.SizeOf<ShaderData>()),
                VkBufferUsageFlags.UniformBuffer
            );
            //setup base ui descriptor set
            _descriptorPool = new API.DescriptorSetPool(Engine.Instance.UiBaseDescriptorLayout);
            _descriptorSet = _descriptorPool.AllocateDescriptorSet();
            _descriptorSet.BuffersUpdate(_descriptorbuffer);

            //setup render command
            _renderCommandPool = new API.CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            _material = UiResources.Materials.Block;
            _isDirty = true;
            Mask = null;
        }

        internal virtual API.BufferTransferObject[] UpdateBuffer()
        {
            try
            {
                // this can set _isDirty to true if the absolute position has changed
                var position = AbsolutePosition;

                if (_isDirty == false)
                    return new API.BufferTransferObject[] { };

                _isDirty = false;
                return new API.BufferTransferObject[]{
                _descriptorbuffer.SetDataGetTransferObject(
                    new ShaderData[]{
                        new ShaderData
                        {
                            Position = position,
                            Scale = Scale,
                            Color = new Vector4(
                                Background.R / 255.0f,
                                Background.G / 255.0f,
                                Background.B / 255.0f,
                                Background.A / 255.0f
                            ),
                            BorderRadius = new Vector4(
                                this.BorderRadiusTopLeft,
                                this.BorderRadiusTopRight,
                                this.BorderRadiusBottomLeft,
                                this.BorderRadiusBottomRight
                            )
                        }
                    }
                )
            };
            }
            catch (System.Exception)
            {
                return new API.BufferTransferObject[0];
            }
        }

        internal virtual Task<API.CommandPool.Command> RecordRenderCommand(Components.Camera camera)
        {
            try
            {
                var descriptorSets = new List<API.DescriptorSetPool.DescriptorSet>();
                descriptorSets.Add(camera.UiDescriptorSet);
                descriptorSets.Add(_descriptorSet);
                foreach (var set in _material.DescriptorSets)
                    descriptorSets.Add(set);

                _material.ReCompilePipeline();
                _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
                _renderCommand.BindPipeline(_material.Pipeline);
                _renderCommand.BindDescriptorSets(_material.Pipeline, descriptorSets.ToArray());
                int viewportX = System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X));
                int viewportY = System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y));
                uint viewportWidth = System.Convert.ToUInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.Z));
                uint viewportHeight = System.Convert.ToUInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.W));
                _renderCommand.SetViewport(viewportX, viewportY, viewportWidth, viewportHeight);
                if (this.Mask == null)
                    _renderCommand.SetScissor(viewportX, viewportY, viewportWidth, viewportHeight);
                else
                {
                    var maskPosition = Mask.AbsolutePosition;
                    var maskScale = Mask.Scale;
                    _renderCommand.SetScissor(
                        System.Convert.ToInt32(System.Math.Round(maskPosition.X)),
                        System.Convert.ToInt32(System.Math.Round(maskPosition.Y)),
                        System.Convert.ToUInt32(System.Math.Round(maskScale.X)),
                        System.Convert.ToUInt32(System.Math.Round(maskScale.Y))
                    );
                }
                _renderCommand.Draw(6);
                _renderCommand.End();
            }
            catch (System.Exception)
            {
                _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
                _renderCommand.End();
            }
            return Task.FromResult(_renderCommand);
        }
    
        /// <summary>
        /// Add a ui element as a child of this ui element
        /// </summary>
        /// <param name="element">element to add as a child</param>
        public override void Add(UiElement element)
        {
            var renderChild = element as UiRenderable;
            if (renderChild != null)
                renderChild.Mask = Mask;
            base.Add(element);
        }
    }
}