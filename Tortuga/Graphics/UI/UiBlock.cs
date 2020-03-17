using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics.UI
{
    public class UiBlock : UiRender
    {
        protected struct ShaderData
        {
            public Vector4 Color;
            public Vector4 Rect;
            public float BorderRadius;
        }

        public Color Background;
        public float BorderRadius;

        private API.Buffer _projectionBuffer;
        private API.Buffer _dataBuffer;

        public UiBlock(Color background)
        {
            //setup descriptor set layout
            this.DescriptorSetLayout = new API.DescriptorSetLayout[]{
                new API.DescriptorSetLayout(
                    new API.DescriptorSetCreateInfo[]{
                        //projection
                        new API.DescriptorSetCreateInfo
                        {
                            stage = VkShaderStageFlags.Vertex,
                            type = VkDescriptorType.UniformBuffer
                        },
                        //data
                        new API.DescriptorSetCreateInfo
                        {
                            stage = VkShaderStageFlags.All,
                            type = VkDescriptorType.UniformBuffer
                        }
                    }
                )
            };

            //setup shader
            this.Shader = new Shader(
                "Assets/Shaders/UI/UI.vert",
                "Assets/Shaders/UI/UI.frag"
            );
            //setup pipeline
            this.PipelineInput = new PipelineInputBuilder();
            this.Pipeline = new API.Pipeline(
                this.DescriptorSetLayout,
                this.Shader.Vertex,
                this.Shader.Fragment,
                this.PipelineInput.BindingDescriptions,
                this.PipelineInput.AttributeDescriptions
            );

            //setup descriptor sets
            this.DescriptorSetPool = new API.DescriptorSetPool[]
            {
                new API.DescriptorSetPool(this.DescriptorSetLayout[0])
            };
            this.DescriptorSet = new API.DescriptorSetPool.DescriptorSet[]
            {
                this.DescriptorSetPool[0].AllocateDescriptorSet()
            };

            //setup buffers
            _projectionBuffer = API.Buffer.CreateDevice(
                System.Convert.ToUInt32(Unsafe.SizeOf<Matrix4x4>()),
                VkBufferUsageFlags.UniformBuffer
            );
            _dataBuffer = API.Buffer.CreateDevice(
                System.Convert.ToUInt32(Unsafe.SizeOf<ShaderData>()),
                VkBufferUsageFlags.UniformBuffer
            );
            this.DescriptorSet[0].BuffersUpdate(_projectionBuffer, 0);
            this.DescriptorSet[0].BuffersUpdate(_dataBuffer, 1);

            this.Background = background;
        }

        internal override BufferTransferObject[] UpdateBuffers()
        {
            var transferObjects = new List<BufferTransferObject>();
            transferObjects.Add(_projectionBuffer.SetDataGetTransferObject(
                new Matrix4x4[]{
                    Matrix4x4.CreateOrthographicOffCenter(
                        0.0f,
                        Engine.Instance.MainWindow.Width,
                        0.0f,
                        Engine.Instance.MainWindow.Height,
                        -1.0f,
                        1.0f
                    )
                }
            ));
            transferObjects.Add(_dataBuffer.SetDataGetTransferObject(
                new ShaderData[]{
                    new ShaderData
                    {
                        BorderRadius = this.BorderRadius,
                        Color = new Vector4(
                            Background.R / 255,
                            Background.G / 255,
                            Background.B / 255,
                            Background.A / 255
                        ),
                        Rect = this.Rect
                    }
                }
            ));
            return transferObjects.ToArray();
        }

        internal override CommandPool.Command BuildRenderCommand(Components.Camera camera)
        {
            this.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
            this.RenderCommand.BindPipeline(this.Pipeline);
            this.RenderCommand.BindDescriptorSets(this.Pipeline, this.DescriptorSet);
            this.RenderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Width))
            );
            this.RenderCommand.Draw(6);
            this.RenderCommand.End();
            return this.RenderCommand;
        }
    }
}