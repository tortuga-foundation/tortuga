using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics.UI
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
        }

        internal virtual API.BufferTransferObject[] UpdateBuffer()
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

        internal virtual Task<API.CommandPool.Command> RecordRenderCommand(Components.Camera camera)
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
            _renderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.Z)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.W))
            );
            _renderCommand.Draw(6);
            _renderCommand.End();
            return Task.FromResult(_renderCommand);
        }
    }
}