using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Vulkan;

namespace Tortuga.Graphics.API
{
    internal class UserInterface
    {
        private const string PROJ_KEY = "PROJECTION";
        private const string FONT_KEY = "FONT";

        public API.Framebuffer Framebuffer => _framebuffer;

        private IntPtr _imGuiContext;
        private API.Buffer _vertexBuffer;
        private API.Buffer _indexBuffer;
        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;
        private API.RenderPass _renderPass;
        private API.Pipeline _pipeline;
        private API.DescriptorSetLayout[] _imGuiDescriptorLayout;
        private DescriptorSetHelper _descriptorHelper;
        private API.CommandPool _commandPool;
        private API.CommandPool.Command _command;
        private API.Framebuffer _framebuffer;
        private uint _width;
        private uint _height;
        private UserInterfaceEventSystem _eventSystem;

        public UserInterface()
        {
            _descriptorHelper = new DescriptorSetHelper();
            _imGuiContext = ImGuiNET.ImGui.CreateContext();

            //setup im gui
            ImGuiNET.ImGui.SetCurrentContext(_imGuiContext);
            ImGuiNET.ImGui.GetIO().Fonts.AddFontDefault();
            ImGuiNET.ImGui.StyleColorsDark();

            //init vertex and index buffers
            _vertexBuffer = API.Buffer.CreateDevice(
                API.Handler.MainDevice,
                10000,
                VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst
            );
            _indexBuffer = API.Buffer.CreateDevice(
                API.Handler.MainDevice,
                2000,
                VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst
            );

            //setup descriptor sets
            _imGuiDescriptorLayout = new API.DescriptorSetLayout[]
            {
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        }
                    }
                )
            };
            //projection binding
            _descriptorHelper.InsertKey(PROJ_KEY, _imGuiDescriptorLayout[0]);
            _descriptorHelper.BindBuffer<Matrix4x4>(PROJ_KEY, 0, null, Unsafe.SizeOf<Matrix4x4>()).Wait();
            //font binding
            _descriptorHelper.InsertKey(FONT_KEY, _imGuiDescriptorLayout[1]);
            _descriptorHelper.BindImage(FONT_KEY, 0, new ShaderPixel[] { ShaderPixel.White }, 1, 1).Wait();

            //setup render pass
            _renderPass = new API.RenderPass(
                API.Handler.MainDevice,
                new API.RenderPass.CreateInfo[]
                {
                    new API.RenderPass.CreateInfo()
                }
            );

            //setup pipeline
            _vertexShader = new API.Shader(
                API.Handler.MainDevice,
                "Assets/Shaders/Default/UI.vert"
            );
            _fragmentShader = new API.Shader(
                API.Handler.MainDevice,
                "Assets/Shaders/Default/UI.frag"
            );
            _pipeline = new API.Pipeline(
                _renderPass,
                _imGuiDescriptorLayout,
                _vertexShader,
                _fragmentShader,
                new PipelineInputBuilder()
                {
                    Bindings = new PipelineInputBuilder.BindingElement[]
                    {
                        new PipelineInputBuilder.BindingElement()
                        {
                            Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                            Elements = new PipelineInputBuilder.AttributeElement[]
                            {
                                //position
                                new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2),
                                //texture coordinate
                                new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Float2),
                                //color
                                new PipelineInputBuilder.AttributeElement(PipelineInputBuilder.AttributeElement.FormatType.Byte4Norm),
                            }
                        }
                    }
                },
                0,
                API.PrimitiveTopology.TriangleList,
                new API.RasterizerInfo(
                    API.RasterizerInfo.FaceCullMode.Back,
                    API.RasterizerInfo.PolygonFillMode.Fill,
                    API.RasterizerInfo.FrontFaceMode.Clockwise,
                    true
                )
            );

            //render command
            _width = 1920;
            _height = 1080;
            _framebuffer = new API.Framebuffer(
                _renderPass, _width, _height
            );
            _commandPool = new API.CommandPool(
                API.Handler.MainDevice,
                API.Handler.MainDevice.GraphicsQueueFamily
            );
            _command = _commandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];

            //setup imgui resources
            this.SetupFonts();
            _eventSystem = new UserInterfaceEventSystem();
        }

        private unsafe void SetupFonts()
        {
            var io = ImGuiNET.ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixel, out int width, out int height, out int bytesPerPixel);

            io.Fonts.SetTexID((IntPtr)1);

            byte[] bytes = new byte[width * height * bytesPerPixel];
            Marshal.Copy((IntPtr)pixel, bytes, 0, bytes.Length);

            _descriptorHelper.BindImage(FONT_KEY, 0, bytes, width, height, bytesPerPixel).Wait();
            io.Fonts.ClearTexData();
        }

        public void NewFrame()
        {
            ImGuiNET.ImGui.NewFrame();
        }

        public unsafe API.BufferTransferObject[] Render(Vector2 canvasSize, API.CommandPool.Command primaryCommand)
        {
            var canvasWidth = Convert.ToInt32(MathF.Round(canvasSize.X));
            var canvasHeight = Convert.ToInt32(MathF.Round(canvasSize.Y));

            var transferCommandList = new List<API.BufferTransferObject>();

            #region fetch dear imGui draw data

            ImGuiNET.ImGui.Render();
            var drawData = ImGuiNET.ImGui.GetDrawData();
            if (drawData.CmdListsCount == 0)
                return new API.BufferTransferObject[] { };

            var io = ImGuiNET.ImGui.GetIO();
            io.DisplaySize = new Vector2(canvasWidth, canvasHeight);
            io.DisplayFramebufferScale = Vector2.One;
            io.DeltaTime = Time.DeltaTime;

            #endregion

            #region update vertex and index buffer size

            var totalVBSize = Convert.ToUInt32(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            if (_vertexBuffer.Size < totalVBSize)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = API.Buffer.CreateDevice(
                    API.Handler.MainDevice,
                    Convert.ToUInt32(MathF.Round(totalVBSize * 1.5f)),
                    VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst
                );
            }

            var totalIBSize = Convert.ToUInt32(drawData.TotalIdxCount * sizeof(ushort));
            if (_indexBuffer.Size < totalIBSize)
            {
                _indexBuffer.Dispose();
                _indexBuffer = API.Buffer.CreateDevice(
                    API.Handler.MainDevice,
                    Convert.ToUInt32(MathF.Round(totalIBSize * 1.5f)),
                    VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst
                );
            }

            #endregion

            #region copy vertex and index buffer data

            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                var cmdList = drawData.CmdListsRange[i];

                transferCommandList.Add(_vertexBuffer.SetDataGetTransferObject(
                    cmdList.VtxBuffer.Data,
                    Convert.ToInt32(vertexOffsetInVertices * Unsafe.SizeOf<ImDrawVert>()),
                    Convert.ToInt32(cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>())
                ));

                transferCommandList.Add(_indexBuffer.SetDataGetTransferObject(
                    cmdList.IdxBuffer.Data,
                    Convert.ToInt32(indexOffsetInElements * sizeof(ushort)),
                    Convert.ToInt32(cmdList.IdxBuffer.Size * sizeof(ushort))
                ));

                vertexOffsetInVertices += (uint)cmdList.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmdList.IdxBuffer.Size;
            }

            #endregion

            #region projection matrix update

            var proj = Matrix4x4.CreateOrthographicOffCenter(0f, canvasWidth, 0.0f, canvasHeight, -1.0f, 1.0f);
            transferCommandList.Add(_descriptorHelper.BindBufferWithTransferObject<Matrix4x4>(PROJ_KEY, 0, new Matrix4x4[] { proj }));

            if (_framebuffer.Width != _width || _framebuffer.Height != _height)
                _framebuffer = new API.Framebuffer(_renderPass, _width, _height);

            #endregion

            #region Render Command

            primaryCommand.BeginRenderPass(_renderPass, _framebuffer);
            _command.Begin(VkCommandBufferUsageFlags.RenderPassContinue, _renderPass, _framebuffer);
            _command.BindPipeline(_pipeline);
            _command.BindDescriptorSets(
                _pipeline,
                new API.DescriptorSetPool.DescriptorSet[]
                {
                    _descriptorHelper.DescriptorObjectMapper[PROJ_KEY].Set,
                    _descriptorHelper.DescriptorObjectMapper[FONT_KEY].Set
                }
            );
            _command.BindVertexBuffer(_vertexBuffer);
            _command.BindIndexBuffer(_indexBuffer);
            _command.SetViewport(0, 0, _framebuffer.Width, _framebuffer.Height);
            int vertexOffset = 0;
            uint indexOffset = 0;
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                var cmdList = drawData.CmdListsRange[i];
                for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
                {
                    var pcmd = cmdList.CmdBuffer[j];
                    if (pcmd.UserCallback != IntPtr.Zero)
                        throw new NotImplementedException("callbacks are not supported");

                    _command.SetScissor(
                        Convert.ToInt32(MathF.Round(pcmd.ClipRect.X)),
                        Convert.ToInt32(MathF.Round(pcmd.ClipRect.Y)),
                        Convert.ToUInt32(MathF.Round(pcmd.ClipRect.Z - pcmd.ClipRect.X)),
                        Convert.ToUInt32(MathF.Round(pcmd.ClipRect.W - pcmd.ClipRect.Y))
                    );
                    _command.DrawIndexed(pcmd.ElemCount, 1, indexOffset, vertexOffset, 0);

                    indexOffset += pcmd.ElemCount;
                }
                vertexOffset += cmdList.VtxBuffer.Size;
            }
            _command.End();
            primaryCommand.ExecuteCommands(new API.CommandPool.Command[] { _command });
            primaryCommand.EndRenderPass();

            #endregion

            return transferCommandList.ToArray();
        }
    }
}