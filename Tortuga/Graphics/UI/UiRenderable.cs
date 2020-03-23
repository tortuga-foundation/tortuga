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
        }

        /// <summary>
        /// Material used for rendering
        /// </summary>
        public UiMaterial Material = new UiMaterial(
            Shader.Load(
                "Assets/Shaders/UI/UI.vert",
                "Assets/Shaders/UI/UI.frag"
            )
        );

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
        }

        internal API.BufferTransferObject UpdateBuffer()
        {
            return _descriptorbuffer.SetDataGetTransferObject(
                new ShaderData[]{
                    new ShaderData
                    {
                        Position = AbsolutePosition,
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
            );
        }

        internal Task<API.CommandPool.Command> RecordRenderCommand(Components.Camera camera)
        {
            var descriptorSets = new List<API.DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.UiDescriptorSet);
            descriptorSets.Add(_descriptorSet);
            foreach (var set in Material.DescriptorSets)
                descriptorSets.Add(set);

            Material.ReCompilePipeline();

            _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
            _renderCommand.BindPipeline(Material.Pipeline);
            _renderCommand.BindDescriptorSets(Material.Pipeline, descriptorSets.ToArray());
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