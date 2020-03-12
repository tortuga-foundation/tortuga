using Vulkan;
using System.Numerics;
using Tortuga.Graphics.API;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Tortuga.Graphics.GUI
{
    public class UserInterface
    {
        public static UserInterface Instance => _instance;

        public List<BaseInterface> Items;

        private static UserInterface _instance;
        private DescriptorSetLayout _descriptorLayout;
        private Shader _shader;
        private PipelineInputBuilder _pipelineInput;
        private Pipeline _pipeline;

        private DescriptorSetPool _descriptorPool;
        private DescriptorSetPool.DescriptorSet _set;
        private API.Buffer _mvpBuffer;
        private API.Image _fontImage;
        private ImageView _fontImageView;
        private Sampler _fontSampler;


        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;

        public UserInterface()
        {
            if (_instance != null)
                throw new System.Exception("This is a singleton please use UserInterface.Instance");
            _instance = this;

            //descriptor layout
            _descriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]{
                new DescriptorSetCreateInfo
                {
                    type = VkDescriptorType.UniformBuffer,
                    stage = VkShaderStageFlags.Vertex
                },
                new DescriptorSetCreateInfo
                {
                    type = VkDescriptorType.CombinedImageSampler,
                    stage = VkShaderStageFlags.Fragment
                }
            });

            //load user interface shader
            _shader = Shader.Load("Assets/Shaders/GUI/GUI.vert", "Assets/Shaders/GUI/GUI.frag");

            //setup pipeline input layout
            _pipelineInput = new PipelineInputBuilder(
                new PipelineInputBuilder.BindingElement[]{
                    new PipelineInputBuilder.BindingElement
                    {
                        Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                        Elements = new PipelineInputBuilder.AttributeElement[]
                        {
                            //vertex position
                            new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2),
                            //vertex uv coordinates
                            new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2)
                        }
                    }
                }
            );

            //setup pipeline
            _pipeline = new Pipeline(
                new DescriptorSetLayout[] { _descriptorLayout },
                _shader.Vertex,
                _shader.Fragment,
                _pipelineInput.BindingDescriptions,
                _pipelineInput.AttributeDescriptions
            );

            //setup mvp buffer
            _mvpBuffer = API.Buffer.CreateHost(
                System.Convert.ToUInt32(Unsafe.SizeOf<Matrix4x4>()),
                VkBufferUsageFlags.UniformBuffer
            );
            _mvpBuffer.SetData(
                new Matrix4x4[]{ 
                    Matrix4x4.CreateOrthographicOffCenter(
                        0f,
                        Engine.Instance.MainWindow.Width,
                        Engine.Instance.MainWindow.Height,
                        0f,
                        -1.0f,
                        1.0f
                    )
                 }
            );

            //setup font image
            _fontImage = new API.Image(
                1, 1,
                VkFormat.R8g8b8a8Srgb,
                VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst
            );
            _fontImageView = new ImageView(
                _fontImage,
                VkImageAspectFlags.Color
            );
            _fontSampler = new Sampler();

            //setup descriptor set
            _descriptorPool = new DescriptorSetPool(_descriptorLayout);
            _set = _descriptorPool.AllocateDescriptorSet();
            _set.BuffersUpdate(_mvpBuffer, 0, 0);
            _set.SampledImageUpdate(_fontImageView, _fontSampler, 0, 1);

            //setup commands
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];

            Items = new List<BaseInterface>();
        }

        internal List<CommandPool.Command> RecordCommands(Components.Camera camera, out CommandPool.Command renderCommand)
        {
            var transferCommands = new List<CommandPool.Command>();

            _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
            _mvpBuffer.SetData(
                new Matrix4x4[]{ 
                    Matrix4x4.CreateOrthographicOffCenter(
                        0f,
                        Engine.Instance.MainWindow.Width,
                        Engine.Instance.MainWindow.Height,
                        0f,
                        -1.0f,
                        1.0f
                    )
                 }
            );
            _renderCommand.BindPipeline(
                _pipeline,
                VkPipelineBindPoint.Graphics,
                new DescriptorSetPool.DescriptorSet[] { _set }
            );
            _renderCommand.SetViewport(
                0, 
                0, 
                System.Convert.ToUInt32(Engine.Instance.MainWindow.Width), 
                System.Convert.ToUInt32(Engine.Instance.MainWindow.Height)
            );
            foreach (var item in Items)
            {
                if (item.IsDirty)
                {
                    foreach (var t in item.TransferCommands)
                        transferCommands.Add(t);
                }

                _renderCommand.BindVertexBuffer(item.VertexBuffer);
                _renderCommand.BindIndexBuffer(item.IndexBuffer);
                _renderCommand.DrawIndexed(item.IndexCount, 1);
            }
            _renderCommand.End();
            renderCommand = _renderCommand;
            return transferCommands;
        }
    }
}