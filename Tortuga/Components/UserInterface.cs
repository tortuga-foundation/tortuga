using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics;
using System.Runtime.CompilerServices;
using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;

namespace Tortuga.Components
{
    public class UserInterface : Core.BaseComponent
    {
        public Graphics.Material ActiveMaterial;

        private static Graphics.API.Buffer _vertexBuffer;
        private static Graphics.API.Buffer _indexBuffer;

        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;

        public enum ShadowType
        {
            Outset = 0,
            Inset = 1
        }

        public class BoxShadow
        {
            public ShadowType Type;
            public Vector2 Offset;
            public float Blur;
            public float Spread;
            public Color Color;
        }

        public Vector2 PositionPixel;

        public Vector2 ScalePixel;

        public bool IsStatic;
        public int IndexZ = 0;
        public float BorderRadius = 0;
        public Graphics.Image Background;
        public BoxShadow Shadow;

        public override async Task OnEnable()
        {
            if (this.ActiveMaterial == null)
            {
                ActiveMaterial = new Material(
                    new Graphics.Shader(
                        "Assets/Shaders/UserInterface/UserInterface.vert",
                        "Assets/Shaders/UserInterface/UserInterface.frag"
                    ),
                    false
                );
                ActiveMaterial.CreateUniformData<ShaderUIStruct>("Data");
                ActiveMaterial.CreateSampledImage("Albedo", new uint[] { 1 });
                ActiveMaterial.UpdateSampledImage("Albedo", 0, Graphics.Image.SingleColor(Color.White)).Wait();
            }
            if (UserInterface._vertexBuffer == null)
            {
                var vertices = new Vertex[]{
                    new Vertex
                    {
                        Position = new Vector3(-1, -1, 0),
                        TextureCoordinates = new Vector2(0, 0),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Vertex
                    {
                        Position = new Vector3(1, -1, 0),
                        TextureCoordinates = new Vector2(1, 0),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Vertex
                    {
                        Position = new Vector3(1, 1, 0),
                        TextureCoordinates = new Vector2(1, 1),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Vertex
                    {
                        Position = new Vector3(-1, 1, 0),
                        TextureCoordinates = new Vector2(0, 1),
                        Normal = new Vector3(0, 0, 1)
                    }
                };

                UserInterface._vertexBuffer = Graphics.API.Buffer.CreateDevice(
                    Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * vertices.Length),
                    VkBufferUsageFlags.VertexBuffer
                );
                await UserInterface._vertexBuffer.SetDataWithStaging(vertices);
            }
            if (UserInterface._indexBuffer == null)
            {
                var indices = new uint[]{
                    0, 1, 2,
                    2, 3, 0
                };

                UserInterface._indexBuffer = Graphics.API.Buffer.CreateDevice(
                    Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * indices.Length),
                    VkBufferUsageFlags.IndexBuffer
                );
                await UserInterface._indexBuffer.SetDataWithStaging(indices);
            }

            _renderCommandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            if (Shadow == null)
                Shadow = new BoxShadow();
        }

        public Task UpdateImage(Graphics.Image image)
        {
            return ActiveMaterial.UpdateSampledImage("Albedo", 0, image);
        }

        internal struct ShaderUIStruct
        {
            public Vector2 Position;
            public Vector2 Scale;
            public int IsStatic;
            public int IndexZ;
            public float BorderRadius;
            public int ShadowType;
            public Vector2 ShadowOffset;
            public float ShadowBlur;
            public float ShadowSpread;
            public Vector4 ShadowColor;
        }
        internal ShaderUIStruct BuildShaderStruct
            => new ShaderUIStruct
            {
                Position = PositionPixel,
                Scale = ScalePixel,
                IsStatic = IsStatic ? 1 : 0,
                IndexZ = IndexZ,
                BorderRadius = BorderRadius,
                ShadowType = (int)Shadow.Type,
                ShadowOffset = Shadow.Offset,
                ShadowBlur = Shadow.Blur,
                ShadowSpread = Shadow.Spread,
                ShadowColor = new Vector4(Shadow.Color.R, Shadow.Color.G, Shadow.Color.B, Shadow.Color.A)
            };

        internal CommandPool.Command BuildDrawCommand(Components.Camera camera)
        {
            _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            _renderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Width))
            );

            var descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.CameraDescriptorSet);
            foreach (var d in ActiveMaterial.DescriptorSets)
                descriptorSets.Add(d);
            ActiveMaterial.ReCompilePipeline();
            _renderCommand.BindPipeline(
                ActiveMaterial.ActivePipeline,
                VkPipelineBindPoint.Graphics,
                descriptorSets.ToArray()
            );
            _renderCommand.BindVertexBuffer(_vertexBuffer);
            _renderCommand.BindIndexBuffer(_indexBuffer);
            _renderCommand.DrawIndexed(6);
            _renderCommand.End();
            return _renderCommand;
        }
    }
}