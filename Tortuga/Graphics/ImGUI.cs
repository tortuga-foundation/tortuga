using System;
using Vulkan;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Tortuga.Graphics
{
    public class ImGUI
    {
        private IntPtr _fontAtlasID = (IntPtr)1;
        private IntPtr _context;
        private ImGuiIOPtr _io;
        private API.Buffer _vertex;
        private API.Buffer _index;
        private API.Buffer _projection;
        private API.Image _fontImage;
        private API.ImageView _fontImageView;
        private API.Sampler _fontSampler;
        private API.DescriptorSetLayout _setLayout;
        private API.DescriptorSetPool _setPool;
        private API.DescriptorSetPool.DescriptorSet _set;
        private Shader _shader;
        private PipelineInputBuilder _pipelineInput;
        private API.Pipeline _pipeline;
        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;

        public ImGUI()
        {
            //setup context
            _context = ImGui.CreateContext();
            ImGui.SetCurrentContext(_context);
            ImGui.StyleColorsDark();

            //setup io
            _io = ImGui.GetIO();
            _io.Fonts.AddFontDefault();

            _vertex = API.Buffer.CreateDevice(
                10000,
                VkBufferUsageFlags.VertexBuffer
            );
            _index = API.Buffer.CreateDevice(
                2000,
                VkBufferUsageFlags.IndexBuffer
            );
            _projection = API.Buffer.CreateDevice(
                64,
                VkBufferUsageFlags.UniformBuffer
            );
            _fontSampler = new API.Sampler();

            //setup descriptor sets
            _setLayout = new API.DescriptorSetLayout(new API.DescriptorSetCreateInfo[]{
                new API.DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Vertex,
                    type = VkDescriptorType.UniformBuffer
                },
                new API.DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Fragment,
                    type = VkDescriptorType.CombinedImageSampler
                }
            });
            _setPool = new API.DescriptorSetPool(_setLayout);
            _set = _setPool.AllocateDescriptorSet();
            _set.BuffersUpdate(_projection, 0, 0);
            SetupFont();

            //load shaders
            _shader = Shader.Load("Assets/Shaders/ImGui/ImGui.vert", "Assets/Shaders/ImGui/ImGui.frag");

            //setup pipeline
            _pipelineInput = new PipelineInputBuilder();
            _pipelineInput.Bindings = new PipelineInputBuilder.BindingElement[]{
                new PipelineInputBuilder.BindingElement
                {
                    Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                    Elements = new PipelineInputBuilder.AttributeElement[]{
                        new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2),
                        new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2),
                        new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float4),
                    }
                }
            };
            _pipeline = new API.Pipeline(
                new API.DescriptorSetLayout[] { _setLayout },
                _shader.Vertex,
                _shader.Fragment,
                _pipelineInput.BindingDescriptions,
                _pipelineInput.AttributeDescriptions
            );

            //create secondary render command
            _renderCommandPool = new API.CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
        }
        ~ImGUI()
        {
            ImGui.DestroyContext(_context);
        }

        private unsafe void SetupFont()
        {
            _io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            _io.Fonts.SetTexID(_fontAtlasID);

            _fontImage = new API.Image(
                System.Convert.ToUInt32(width),
                System.Convert.ToUInt32(height),
                VkFormat.R8g8b8a8Unorm,
                VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst
            );
            _fontImageView = new API.ImageView(_fontImage, VkImageAspectFlags.Color);
            _set.SampledImageUpdate(_fontImageView, _fontSampler, 0, 1);
            var copyBuffer = API.Buffer.CreateHost(
                System.Convert.ToUInt32(width * height * bytesPerPixel),
                VkBufferUsageFlags.TransferSrc
            );
            copyBuffer.SetData((IntPtr)pixels, 0, System.Convert.ToInt32(copyBuffer.Size));
            var fence = new API.Fence();
            var cmdPool = new API.CommandPool(Engine.Instance.MainDevice.TransferQueueFamily);
            var cmd = cmdPool.AllocateCommands()[0];
            cmd.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            cmd.TransferImageLayout(_fontImage, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
            cmd.BufferToImage(copyBuffer, _fontImage);
            cmd.TransferImageLayout(_fontImage, VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal);
            cmd.End();
            cmd.Submit(Engine.Instance.MainDevice.TransferQueueFamily.Queues[0], null, null, fence);
            fence.Wait();

            _io.Fonts.ClearTexData();
        }


        internal struct RenderCommandResponse
        {
            public API.CommandPool.Command RenderCommand;
            public List<API.BufferTransferObject> TransferCommands;
        }

        internal Task<RenderCommandResponse> RenderCommand(Components.Camera camera)
        {
            ImGui.NewFrame();

            ImGui.Begin("Test");
            ImGui.Text("Hello World");
            ImGui.End();

            ImGui.Render();
            var transferCommands = new List<API.BufferTransferObject>();

            //setup im gui per frame data
            _io.DisplaySize = new Vector2(
                camera.Resolution.x,
                camera.Resolution.y
            );
            _io.DisplayFramebufferScale = Vector2.One;
            _io.DeltaTime = Time.DeltaTime;

            //record secondary render command
            _renderCommand.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                camera.Framebuffer
            );

            //get ImGui draw data
            var drawData = ImGui.GetDrawData();
            if (drawData.CmdListsCount > 0)
            {

                //resize vertex buffer
                uint totalVBSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
                if (totalVBSize != _vertex.Size)
                {
                    _vertex = API.Buffer.CreateDevice(
                        Convert.ToUInt32(MathF.Round(totalVBSize)),
                        VkBufferUsageFlags.VertexBuffer
                    );
                }

                //resize index buffer
                uint totalIBSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
                if (totalIBSize != _index.Size)
                {
                    _index = API.Buffer.CreateDevice(
                        Convert.ToUInt32(MathF.Round(totalVBSize)),
                        VkBufferUsageFlags.IndexBuffer
                    );
                }

                //update buffer data
                int vertexDataOffset = 0;
                int indexDataOffset = 0;
                for (int i = 0; i < drawData.CmdListsCount; i++)
                {
                    var cmdList = drawData.CmdListsRange[i];

                    transferCommands.Add(
                        _vertex.SetDataGetTransferObject(
                            cmdList.VtxBuffer.Data,
                            vertexDataOffset,
                            cmdList.VtxBuffer.Size
                        )
                    );
                    transferCommands.Add(
                        _index.SetDataGetTransferObject(
                            cmdList.IdxBuffer.Data,
                            indexDataOffset,
                            cmdList.IdxBuffer.Size
                        )
                    );
                    vertexDataOffset += cmdList.VtxBuffer.Size;
                    indexDataOffset += cmdList.IdxBuffer.Size;
                }

                var mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    _io.DisplaySize.X,
                    _io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f
                );
                transferCommands.Add(
                    _projection.SetDataGetTransferObject(
                        new Matrix4x4[] { mvp }
                    )
                );

                _renderCommand.BindVertexBuffer(_vertex);
                _renderCommand.BindIndexBuffer(_index);
                _renderCommand.BindPipeline(
                    _pipeline,
                    VkPipelineBindPoint.Graphics,
                    new API.DescriptorSetPool.DescriptorSet[]{
                    _set
                });

                uint idx_offset = 0;
                int vtx_offset = 0;
                for (int n = 0; n < drawData.CmdListsCount; n++)
                {
                    var cmd_list = drawData.CmdListsRange[n];
                    for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                    {
                        var pcmd = cmd_list.CmdBuffer[cmd_i];
                        if (pcmd.UserCallback != IntPtr.Zero)
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            _renderCommand.SetViewport(
                                System.Convert.ToInt32(pcmd.ClipRect.X),
                                System.Convert.ToInt32(pcmd.ClipRect.Y),
                                System.Convert.ToUInt32(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                                System.Convert.ToUInt32(pcmd.ClipRect.W - pcmd.ClipRect.Y)
                            );
                            _renderCommand.DrawIndexed(pcmd.ElemCount, 1, idx_offset, vtx_offset, 0);
                        }
                        idx_offset += pcmd.ElemCount;
                    }
                    vtx_offset += cmd_list.VtxBuffer.Size;
                }
            }
            _renderCommand.End();
            ImGui.EndFrame();

            return Task.FromResult(new RenderCommandResponse
            {
                RenderCommand = _renderCommand,
                TransferCommands = transferCommands
            });
        }
    }
}